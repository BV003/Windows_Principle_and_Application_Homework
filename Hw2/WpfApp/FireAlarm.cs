using System;
using System.Windows.Controls;

namespace WpfApp
{
    public class FireAlarmEventArgs : EventArgs
    {
        public string Location { get; set; }
        public int Severity { get; set; }

        public FireAlarmEventArgs(string location, int severity)
        {
            Location = location;
            Severity = severity;
        }
    }

    public class FireAlarm
    {
        public delegate void FireAlarmEventHandler(object sender, FireAlarmEventArgs e);
        public event FireAlarmEventHandler FireEvent;

        public void TriggerFireAlarm(string location, int severity)
        {
            FireEvent?.Invoke(this, new FireAlarmEventArgs(location, severity));
        }
    }

    public class FireDepartment
    {
        private MainWindow _mainWindow;

        public FireDepartment(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }
        public void OnFireAlarmRaised(object sender, FireAlarmEventArgs e)
        {
            _mainWindow.AppendTextToTextBox($"Fire in {e.Location} with severity {e.Severity}. Fire Department responding.");
        }
    }

    public class SecurityTeam
    {
        private MainWindow _mainWindow;

        public SecurityTeam(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void OnFireAlarmRaised(object sender, FireAlarmEventArgs e)
        {
            _mainWindow.AppendTextToTextBox($"Fire in {e.Location} with severity {e.Severity}. Security Team responding.");
        }
    }


}
