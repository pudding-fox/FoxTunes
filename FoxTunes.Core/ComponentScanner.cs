using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FoxTunes
{
    public class ComponentScanner : BaseComponent, IComponentScanner
    {
        const FileSystemHelper.SearchOption SEARCH_OPTIONS =
            FileSystemHelper.SearchOption.Recursive |
            FileSystemHelper.SearchOption.UseSystemCache |
            FileSystemHelper.SearchOption.UseSystemExclusions;

        private static Type[] STANDARD_COMPONENT_TYPES = new[]
        {
            typeof(IStandardComponent),
            typeof(IStandardManager),
            typeof(IStandardFactory),
            typeof(IStandardBehaviour)
        };

        private ComponentScanner()
        {
            this._StandardComponents = new Lazy<IEnumerable<Type>>(GetAndResolveStandardComponents);
        }

        public static string Location
        {
            get
            {
                return Path.GetDirectoryName(typeof(ComponentScanner).Assembly.Location);
            }
        }

        private static IEnumerable<string> GetFileNames()
        {
            return Enumerable.Concat(
                FileSystemHelper.EnumerateFiles(Location, "FoxTunes*.dll", SEARCH_OPTIONS),
                FileSystemHelper.EnumerateFiles(Location, "FoxTunes*.exe", SEARCH_OPTIONS)
            ).ToArray();
        }

        private static IEnumerable<Type> GetTypes(string fileName)
        {
            var assemblyName = AssemblyRegistry.Instance.GetAssemblyName(fileName);
            if (assemblyName == null)
            {
                return Enumerable.Empty<Type>();
            }
            var assembly = default(Assembly);
            try
            {
                assembly = AssemblyRegistry.Instance.GetOrLoadReflectionAssembly(fileName);
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types;
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }
        }

        private static IEnumerable<Type> GetAndResolveStandardComponents()
        {
            var slots = new Dictionary<string, IList<Type>>(StringComparer.OrdinalIgnoreCase);
            try
            {
                return GetFileNames()
                    .SelectMany(GetTypes)
                    .Where(ComponentFilter.IsStandardComponent)
                    .Where(ComponentFilter.IsCompatable)
                    .Where(ComponentFilter.IsSelected)
                    .OrderBy(ComponentSorter.Type)
                    .ThenBy(ComponentSorter.Preference)
                    .ThenBy(ComponentSorter.Priority)
                    .Where(ComponentFilter.IsFreeSlot(slots))
                    .ToArray();
            }
            finally
            {
                foreach (var pair in slots)
                {
                    if (pair.Value.Count <= 1)
                    {
                        continue;
                    }
                    ComponentResolver.Instance.Add(pair.Key, pair.Value[0], pair.Value);
                }
            }
        }

        private Lazy<IEnumerable<Type>> _StandardComponents { get; set; }

        public IEnumerable<Type> GetStandardComponents()
        {
            return this._StandardComponents.Value;
        }

        public IEnumerable<Type> GetComponents(Type type)
        {
            return GetFileNames()
                .SelectMany(GetTypes)
                .Where(ComponentFilter.IsComponent(type))
                .Where(ComponentFilter.IsCompatable)
                .OrderBy(ComponentSorter.Type)
                .ThenBy(ComponentSorter.Preference)
                .ThenBy(ComponentSorter.Priority)
                .ToArray();
        }

        public static readonly IComponentScanner Instance = new ComponentScanner();

        public static bool ImplementsInterface(Type type, Type @interface)
        {
            return ImplementsInterface(type, @interface.FullName);
        }

        public static bool ImplementsInterface(Type type, string @interface)
        {
            return type.GetInterfaces().Any(interfaceType => string.Equals(interfaceType.FullName, @interface, StringComparison.OrdinalIgnoreCase));
        }

        public static bool ImplementsInterface(Type type, IEnumerable<Type> interfaces)
        {
            return ImplementsInterface(type, interfaces.Select(@interface => @interface.FullName));
        }

        public static bool ImplementsInterface(Type type, IEnumerable<string> interfaces)
        {
            return type.GetInterfaces().Any(interfaceType => interfaces.Contains(interfaceType.FullName, StringComparer.OrdinalIgnoreCase));
        }

        private static class ComponentFilter
        {
            public static Func<Type, bool> IsComponent(Type @interface)
            {
                return type => type.IsClass && !type.IsAbstract && ImplementsInterface(type, @interface);
            }

            public static bool IsStandardComponent(Type type)
            {
                return type.IsClass && !type.IsAbstract && ImplementsInterface(type, STANDARD_COMPONENT_TYPES);
            }

            public static bool IsCompatable(Type type)
            {
                return HasPlatform(type) && HasSlots(type);
            }

            public static bool HasPlatform(Type type)
            {
                var dependencies = default(IEnumerable<PlatformDependencyAttribute>);
                if (type.HasCustomAttributes<PlatformDependencyAttribute>(out dependencies))
                {
                    foreach (var dependency in dependencies)
                    {
                        var version = new Version(dependency.Major, dependency.Minor);
                        if (Environment.OSVersion.Version < version)
                        {
                            return false;
                        }
                        if (dependency.Architecture != ProcessorArchitecture.None)
                        {
                            var is64BitProcess = Environment.Is64BitProcess;
                            var is34BitProcess = !is64BitProcess;
                            if (dependency.Architecture == ProcessorArchitecture.X86 && is64BitProcess)
                            {
                                return false;
                            }
                            if (dependency.Architecture == ProcessorArchitecture.X64 && !is34BitProcess)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }

            public static bool HasSlots(Type type)
            {
                var dependencies = default(IEnumerable<ComponentDependencyAttribute>);
                if (type.HasCustomAttributes<ComponentDependencyAttribute>(out dependencies))
                {
                    foreach (var dependency in dependencies)
                    {
                        if (!string.IsNullOrEmpty(dependency.Slot))
                        {
                            var id = ComponentResolver.Instance.Get(dependency.Slot);
                            if (string.Equals(id, ComponentSlots.Blocked, StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }

            public static bool IsSelected(Type type)
            {
                var component = default(ComponentAttribute);
                if (!type.HasCustomAttribute<ComponentAttribute>(out component))
                {
                    return true;
                }
                if (string.IsNullOrEmpty(component.Slot))
                {
                    return true;
                }
                var id = ComponentResolver.Instance.Get(component.Slot);
                if (string.Equals(id, ComponentSlots.None, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (string.Equals(id, component.Id, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return false;
            }

            public static Func<Type, bool> IsFreeSlot(IDictionary<string, IList<Type>> slots)
            {
                return type =>
                {
                    var component = default(ComponentAttribute);
                    if (!type.HasCustomAttribute<ComponentAttribute>(out component))
                    {
                        return true;
                    }
                    if (string.IsNullOrEmpty(component.Slot) || string.Equals(component.Slot, ComponentSlots.None, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    var components = slots.GetOrAdd(component.Slot, () => new List<Type>());
                    components.Add(type);
                    if (components.Count == 1)
                    {
                        return true;
                    }
                    return false;
                };
            }
        }

        private static class ComponentSorter
        {
            public static byte Type(Type type)
            {
                var interfaces = type.GetInterfaces().Select(
                    @interface => @interface.FullName
                ).ToArray();
                if (interfaces.Contains(typeof(IStandardComponent).FullName, StringComparer.OrdinalIgnoreCase))
                {
                    return 0;
                }
                if (interfaces.Contains(typeof(IStandardFactory).FullName, StringComparer.OrdinalIgnoreCase))
                {
                    return 1;
                }
                if (interfaces.Contains(typeof(IStandardManager).FullName, StringComparer.OrdinalIgnoreCase))
                {
                    return 2;
                }
                if (interfaces.Contains(typeof(IStandardBehaviour).FullName, StringComparer.OrdinalIgnoreCase))
                {
                    return 3;
                }
                return byte.MaxValue;
            }

            public static byte Priority(Type type)
            {
                var priority = default(ComponentPriorityAttribute);
                if (!type.HasCustomAttribute<ComponentPriorityAttribute>(out priority))
                {
                    return ComponentPriorityAttribute.NORMAL;
                }
                return priority.Priority;
            }

            public static byte Preference(Type type)
            {
                var preference = default(ComponentPreferenceAttribute);
                if (!type.HasCustomAttribute<ComponentPreferenceAttribute>(out preference))
                {
                    return ComponentPreferenceAttribute.NORMAL;
                }
                return preference.Priority;
            }
        }
    }
}
