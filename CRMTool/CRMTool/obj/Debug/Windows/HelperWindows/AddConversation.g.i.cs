﻿#pragma checksum "..\..\..\..\Windows\HelperWindows\AddConversation.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "BFA61E73C6A0A312AE39E2E5672D265B"
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
using Licencjat_new.Windows.HelperWindows;
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
using WpfAnimatedGif;


namespace Licencjat_new.Windows.HelperWindows {
    
    
    /// <summary>
    /// AddConversation
    /// </summary>
    public partial class AddConversation : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 14 "..\..\..\..\Windows\HelperWindows\AddConversation.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DockPanel MainDock;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\..\Windows\HelperWindows\AddConversation.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid FirstStep;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\..\Windows\HelperWindows\AddConversation.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Licencjat_new.Controls.RoundedTextBox NameText;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\..\Windows\HelperWindows\AddConversation.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Licencjat_new.Controls.RoundedButton ReadyButton;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\..\Windows\HelperWindows\AddConversation.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Licencjat_new.Controls.RoundedButton CancelButton;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\..\Windows\HelperWindows\AddConversation.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label ErrorLabel;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\..\Windows\HelperWindows\AddConversation.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border loadingOverlay;
        
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
            System.Uri resourceLocater = new System.Uri("/Licencjat_new;component/windows/helperwindows/addconversation.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Windows\HelperWindows\AddConversation.xaml"
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
            this.MainDock = ((System.Windows.Controls.DockPanel)(target));
            return;
            case 2:
            this.FirstStep = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.NameText = ((Licencjat_new.Controls.RoundedTextBox)(target));
            return;
            case 4:
            this.ReadyButton = ((Licencjat_new.Controls.RoundedButton)(target));
            return;
            case 5:
            this.CancelButton = ((Licencjat_new.Controls.RoundedButton)(target));
            return;
            case 6:
            this.ErrorLabel = ((System.Windows.Controls.Label)(target));
            return;
            case 7:
            this.loadingOverlay = ((System.Windows.Controls.Border)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
