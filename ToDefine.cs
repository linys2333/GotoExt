//------------------------------------------------------------------------------
// <copyright file="ToDefine.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ToAPI
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ToDefine
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int ToDefineCmdId = 0x0100;
        public const int ToSQLCmdId = 0x0200;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("c1597039-3473-40d8-8fb0-792f9f126f7f");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDefine"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ToDefine(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var toDefineCmdId = new CommandID(CommandSet, ToDefineCmdId);
                var toDefineItem = new MenuCommand(this.ToDefineItemCallback, toDefineCmdId);
                commandService.AddCommand(toDefineItem);

                var toSQLCmdId = new CommandID(CommandSet, ToSQLCmdId);
                var toSQLItem = new MenuCommand(this.ToSQLItemCallback, toSQLCmdId);
                commandService.AddCommand(toSQLItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ToDefine Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new ToDefine(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void ToDefineItemCallback(object sender, EventArgs e)
        {
            DTE2 dte = (DTE2) this.ServiceProvider.GetService(typeof(DTE));

            try
            {
                new ToDefineBiz(dte).Go();
            }
            catch
            {
                // ignored
            }
        }

        private void ToSQLItemCallback(object sender, EventArgs e)
        {
            DTE2 dte = (DTE2)this.ServiceProvider.GetService(typeof(DTE));

            try
            {
                new ToSQLBiz(dte).Go();
            }
            catch
            {
                // ignored
            }
        }
    }
}
