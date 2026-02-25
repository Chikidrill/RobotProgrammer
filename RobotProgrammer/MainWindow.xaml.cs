using Model.RobotActions;
using RobotProgrammer.ViewModel;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RobotProgrammer.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _dragStart;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainVM(new FileDialogService(), new WindowService(), new DialogService());
            
        }
        private void Tree_SelectedItemChanged(object sender,
    RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainVM vm)
                vm.SelectedAction = e.NewValue as RobotAction;
        }
        private void Tree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(null);
        }
        private void Tree_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var currentPosition = e.GetPosition(null);
            var diff = _dragStart - currentPosition;

            // Минимальное смещение чтобы не дергало при клике
            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            // Получаем TreeView и выбранный элемент
            if (sender is not TreeView tree)
                return;

            if (tree.SelectedItem is RobotAction dragged)
            {
                // Запускаем DragDrop
                DataObject data = new DataObject(typeof(RobotAction), dragged);
                DragDrop.DoDragDrop(tree, data, DragDropEffects.Move);
            }
        }
        private void Tree_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(RobotAction)))
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            var dragged = (RobotAction)e.Data.GetData(typeof(RobotAction));
            var target = GetActionUnderMouse(e);

            if (target == null)
            {
                e.Effects = DragDropEffects.Move;
                return;
            }

            if (dragged == target)
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            if (dragged is ContainerAction container &&
                container.Contains(target))
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            e.Effects = DragDropEffects.Move;
        }
        private void Tree_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(RobotAction)))
                return;

            var dragged = (RobotAction)e.Data.GetData(typeof(RobotAction));
            var target = GetActionUnderMouse(e);

            if (dragged == target)
                return;

            RemoveFromParent(dragged);

            if (target is ContainerAction container)
            {
                container.Children.Add(dragged);
                dragged.Parent = container;
            }
            else if (target != null)
            {
                if (target.Parent != null)
                {
                    var parent = target.Parent;
                    int index = parent.Children.IndexOf(target);
                    parent.Children.Insert(index + 1, dragged);
                    dragged.Parent = parent;
                }
                else
                {
                    var vm = (MainVM)DataContext;
                    int index = vm.Actions.IndexOf(target);
                    vm.Actions.Insert(index + 1, dragged);
                    dragged.Parent = null;
                }
            }
            else
            {
                var vm = (MainVM)DataContext;
                vm.Actions.Add(dragged);
                dragged.Parent = null;
            }
            ((MainVM)DataContext).UpdatePreview();
        }
        private void RemoveFromParent(RobotAction action)
        {
            var vm = (MainVM)DataContext;

            if (action.Parent != null)
            {
                action.Parent.Children.Remove(action);
            }
            else
            {
                vm.Actions.Remove(action);
            }

            vm.UpdatePreview(); // вызывается ОДИН раз корректно
        }
        private IEnumerable<ContainerAction> GetAllContainers(IEnumerable<RobotAction> actions)
        {
            foreach (var a in actions)
            {
                if (a is ContainerAction c)
                {
                    yield return c;

                    foreach (var child in GetAllContainers(c.Children))
                        yield return child;
                }
            }
        }
        private RobotAction? GetActionUnderMouse(DragEventArgs e)
        {
            var element = ActionsTree.InputHitTest(
                e.GetPosition(ActionsTree)) as DependencyObject;

            while (element != null)
            {
                if (element is TreeViewItem item)
                    return item.DataContext as RobotAction;

                element = VisualTreeHelper.GetParent(element);
            }

            return null;
        }
       
    }
}
