using FoxTunes.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxTunes
{
    public abstract class UserInterface : StandardComponent, IUserInterface
    {
        public abstract IEnumerable<IUserInterfaceWindow> Windows { get; }

        public abstract Task Show();

        public abstract void Activate();

        public abstract void Warn(string message);

        public abstract void Fatal(Exception exception);

        public abstract bool Confirm(string message);

        public abstract string Prompt(string message);

        public abstract void Restart();

        protected virtual void OnWindowCreated(IUserInterfaceWindow window)
        {
            if (this.WindowCreated == null)
            {
                return;
            }
            this.WindowCreated(this, new UserInterfaceWindowEventArgs(window));
        }

        public event UserInterfaceWindowEventHandler WindowCreated;

        protected virtual void OnWindowDestroyed(IUserInterfaceWindow window)
        {
            if (this.WindowDestroyed == null)
            {
                return;
            }
            this.WindowDestroyed(this, new UserInterfaceWindowEventArgs(window));
        }

        public event UserInterfaceWindowEventHandler WindowDestroyed;
    }
}
