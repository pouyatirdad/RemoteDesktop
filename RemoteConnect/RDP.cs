using AxMSTSCLib;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteConnect
{
    public class RDP
    {
        #region Methods

        public void Connect(string username, string password, string machineName)
        {
            try
            {
                var form = new Form();
                var remoteDesktopClient = new AxMsRdpClient6NotSafeForScripting();
                form.Controls.Add(remoteDesktopClient);
                form.Show();

                remoteDesktopClient.AdvancedSettings7.AuthenticationLevel = 0;
                remoteDesktopClient.AdvancedSettings7.EnableCredSspSupport = true;
                remoteDesktopClient.Server = machineName;
                remoteDesktopClient.UserName = username;
                remoteDesktopClient.AdvancedSettings7.ClearTextPassword = password;
                remoteDesktopClient.Connect();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        #endregion

        #region Nested type: MessageLoopApartment

        public class MessageLoopApartment : IDisposable
        {
            #region  Fields/Consts

            private static readonly Lazy<MessageLoopApartment> Instance = new Lazy<MessageLoopApartment>(() => new MessageLoopApartment());
            private TaskScheduler _taskScheduler;
            private Thread _thread;

            #endregion

            #region  Properties

            public static MessageLoopApartment I => Instance.Value;

            #endregion

            private MessageLoopApartment()
            {
                var tcs = new TaskCompletionSource<TaskScheduler>();

                _thread = new Thread(startArg =>
                {
                    void IdleHandler(object s, EventArgs e)
                    {
                        System.Windows.Forms.Application.Idle -= IdleHandler;
                        tcs.SetResult(TaskScheduler.FromCurrentSynchronizationContext());
                    }

                    System.Windows.Forms.Application.Idle += IdleHandler;
                    System.Windows.Forms.Application.Run();
                });

                _thread.SetApartmentState(ApartmentState.STA);
                _thread.IsBackground = true;
                _thread.Start();
                _taskScheduler = tcs.Task.Result;
            }

            #region IDisposable Implementation

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Methods

            public Task Run(Action action, CancellationToken token)
            {
                return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }, token, TaskCreationOptions.LongRunning, _taskScheduler);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (_taskScheduler == null) return;

                var taskScheduler = _taskScheduler;
                _taskScheduler = null;
                Task.Factory.StartNew(
                        System.Windows.Forms.Application.ExitThread,
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        taskScheduler)
                    .Wait();
                _thread.Join();
                _thread = null;
            }

            #endregion
        }

        #endregion
    }
}
