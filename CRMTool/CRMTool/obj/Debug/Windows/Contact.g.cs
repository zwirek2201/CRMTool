﻿#pragma checksum "..\..\..\Windows\Contact.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "3A54F5FCE24BA2756BBB98B9689675DD"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Licencjat_new.Controls;
using Licencjat_new.Windows;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Licencjat_new.Windows {
    
    
    /// <summary>
    /// Contact
    /// </summary>
    public partial class Contact : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 11 "..\..\..\Windows\Contact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid MainDockPanel;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\Windows\Contact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Licencjat_new.Controls.ContactSearchBox ContactSearchBox;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\Windows\Contact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Licencjat_new.Controls.ContactMainContainer ContactMainContainer;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Windows\Contact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Licencjat_new.Controls.ContactTabControl ContactTabControl;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\Windows\Contact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Licencjat_new.Controls.ToolBarMainMenuStrip MainMenuStrip;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\Windows\Contact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Licencjat_new.Controls.ToolBarToggleButton ToggleButton;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\Windows\Contact.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Licencjat_new.Controls.AlphabetList AlphabetList;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Licencjat_new;component/windows/contact.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Windows\Contact.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.MainDockPanel = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.ContactSearchBox = ((Licencjat_new.Controls.ContactSearchBox)(target));
            return;
            case 3:
            this.ContactMainContainer = ((Licencjat_new.Controls.ContactMainContainer)(target));
            return;
            case 4:
            this.ContactTabControl = ((Licencjat_new.Controls.ContactTabControl)(target));
            return;
            case 5:
            this.MainMenuStrip = ((Licencjat_new.Controls.ToolBarMainMenuStrip)(target));
            return;
            case 6:
            this.ToggleButton = ((Licencjat_new.Controls.ToolBarToggleButton)(target));
            return;
            case 7:
            this.AlphabetList = ((Licencjat_new.Controls.AlphabetList)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
