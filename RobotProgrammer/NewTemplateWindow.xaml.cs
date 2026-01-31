using RobotProgrammer.Model;
using System.Collections.Generic;
using System.Windows;

namespace RobotProgrammer.View
{
    public partial class NewTemplateWindow : Window
    {
        public CustomAction Result { get; private set; }

        public NewTemplateWindow()
        {
            InitializeComponent();
            ParamsGrid.ItemsSource = new List<KeyValuePair<string, int>>();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var template = new CustomAction
            {
                TemplateName = TemplateNameBox.Text,
                TemplateCode = CodeBox.Text,
                Parameters = new Dictionary<string, int>()
            };

            foreach (var item in ParamsGrid.Items)
            {
                if (item is KeyValuePair<string, int> kv)
                {
                    template.Parameters[kv.Key] = kv.Value;
                }
            }

            TemplateService.SaveTemplate(template);
            Result = template;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
