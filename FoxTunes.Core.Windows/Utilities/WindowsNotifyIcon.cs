using FoxTunes.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace FoxTunes
{
    [ComponentDependency(Slot = ComponentSlots.UserInterface)]
    public class WindowsNotifyIcon : NotifyIcon, IStandardComponent
    {
        public const uint ID = 0x400;

        public static readonly object SyncRoot = new object();

        public override IntPtr Icon { get; set; }

        public override IMessageSink MessageSink { get; protected set; }

        public IMessageSinkFactory MessageSinkFactory { get; private set; }

        public bool IsVisible { get; private set; }

        public override void InitializeComponent(ICore core)
        {
            this.MessageSinkFactory = ComponentRegistry.Instance.GetComponent<IMessageSinkFactory>();
            base.InitializeComponent(core);
        }

        protected virtual void EnsureMessageSink()
        {
            if (this.MessageSink == null)
            {
                this.MessageSink = this.MessageSinkFactory.Create(ID);
            }
        }

        public override void Show()
        {
            lock (SyncRoot)
            {
                if (this.IsVisible)
                {
                    return;
                }
                this.EnsureMessageSink();
                var data = NotifyIconData.Create(ID, this.MessageSink.Handle, this.Icon);
                if (ShellNotifyIcon(NotifyCommand.Add, ref data))
                {
                    this.IsVisible = true;
                }
            }
        }

        public override bool Update()
        {
            lock (SyncRoot)
            {
                if (!this.IsVisible)
                {
                    return false;
                }
                var data = NotifyIconData.Create(ID, this.MessageSink.Handle, this.Icon);
                return ShellNotifyIcon(NotifyCommand.Modify, ref data);
            }
        }

        public override void Hide()
        {
            lock (SyncRoot)
            {
                if (!this.IsVisible)
                {
                    return;
                }
                var data = NotifyIconData.Create(ID, this.MessageSink.Handle, this.Icon);
                if (ShellNotifyIcon(NotifyCommand.Delete, ref data))
                {
                    this.IsVisible = false;
                }
            }
        }

        protected override void OnDisposing()
        {
            this.Hide();
            base.OnDisposing();
        }

        public enum IconState
        {
            Visible = 0x00,
            Hidden = 0x01,
            Shared = 0x02
        }

        [Flags]
        public enum IconDataMembers
        {
            Message = 0x01,
            Icon = 0x02,
            Tip = 0x04,
            State = 0x08,
            Info = 0x10,
            Realtime = 0x40,
            UseLegacyToolTips = 0x80
        }

        public enum BalloonFlags
        {
            None = 0x00,
            Info = 0x01,
            Warning = 0x02,
            Error = 0x03,
            User = 0x04,
            NoSound = 0x10,
            LargeIcon = 0x20,
            RespectQuietTime = 0x80
        }

        public enum NotifyIconVersion
        {
            Win95 = 0x0,
            Win2000 = 0x3,
            Vista = 0x4
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct NotifyIconData
        {
            public uint cbSize;

            public IntPtr WindowHandle;

            public uint TaskbarIconId;

            public IconDataMembers ValidMembers;

            public uint CallbackMessageId;

            public IntPtr IconHandle;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string ToolTipText;

            public IconState IconState;

            public IconState StateMask;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string BalloonText;

            public uint VersionOrTimeout;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string BalloonTitle;

            public BalloonFlags BalloonFlags;

            public Guid TaskbarIconGuid;

            public IntPtr CustomBalloonIconHandle;

            public static NotifyIconData Create(uint id, IntPtr windowHandle, IntPtr iconHandle)
            {
                var data = new NotifyIconData();
                data.cbSize = (uint)Marshal.SizeOf(data);

                data.WindowHandle = windowHandle;
                data.IconHandle = iconHandle;

                data.TaskbarIconId = 0x0;
                data.CallbackMessageId = id;
                data.VersionOrTimeout = (uint)NotifyIconVersion.Vista;

                data.IconState = IconState.Hidden;
                data.StateMask = IconState.Hidden;
                data.ValidMembers = IconDataMembers.Message | IconDataMembers.Icon | IconDataMembers.Tip;

                data.ToolTipText = string.Empty;
                data.BalloonText = string.Empty;
                data.BalloonTitle = string.Empty;

                return data;
            }
        }

        public enum NotifyCommand
        {
            Add = 0x00,
            Modify = 0x01,
            Delete = 0x02,
            SetFocus = 0x03,
            SetVersion = 0x04
        }

        [DllImport("shell32.dll", EntryPoint = "Shell_NotifyIcon", CharSet = CharSet.Unicode)]
        public static extern bool ShellNotifyIcon(NotifyCommand cmd, [In] ref NotifyIconData data);
    }
}
