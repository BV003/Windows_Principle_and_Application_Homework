// MainWindow.xaml.cs
using System;
using System.Windows;

namespace WpfApp
{

    public partial class MainWindow : Window
    {
        private FireAlarm fireAlarm;
        private FireDepartment fireDepartment;
        private SecurityTeam securityTeam;

        public MainWindow()
        {
            InitializeComponent();
            fireAlarm = new FireAlarm();
            fireDepartment = new FireDepartment(this);
            securityTeam = new SecurityTeam(this);
        }
        public void AppendTextToTextBox(string message)
        {
            OutputTextBox.AppendText(message + "\n");
        }

        private void Bind1()
        {
            fireAlarm.FireEvent += fireDepartment.OnFireAlarmRaised;
        }

        private void Bind2()
        {
            fireAlarm.FireEvent += securityTeam.OnFireAlarmRaised;
        }

        private void Unbind1()
        {
            fireAlarm.FireEvent -= fireDepartment.OnFireAlarmRaised;
        }

        private void Unbind2()
        {
            fireAlarm.FireEvent -= securityTeam.OnFireAlarmRaised;
        }

        private void BtnTrigger_Click(object sender, RoutedEventArgs e)
        {
            fireAlarm.TriggerFireAlarm("Main Building", 5);
        }

        private void Bind1_Click(object sender, RoutedEventArgs e)
        {
            Bind1();
            OutputTextBox.AppendText("FireDepartment 绑定成功\n");
        }

        private void Bind2_Click(object sender, RoutedEventArgs e)
        {
            Bind2();
            OutputTextBox.AppendText("SecurityTeam 绑定成功\n");
        }

        private void Unbind1_Click(object sender, RoutedEventArgs e)
        {
            Unbind1();
            OutputTextBox.AppendText("FireDepartment 解绑成功\n");
        }

        private void Unbind2_Click(object sender, RoutedEventArgs e)
        {
            Unbind2();
            OutputTextBox.AppendText("SecurityTeam 解绑成功\n");
        }
    }
}