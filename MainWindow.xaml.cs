using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HiszpanskiWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is Lesson selectedLesson)
            {
                Console.WriteLine($"Wybrano lekcję: {selectedLesson.Title}");
            }
        }

        private void UserAnswerTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Sprawdzamy, czy jest wciśnięty Ctrl (lewy lub prawy), ale NIE Alt
            bool isCtrlPressed =
                (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                !Keyboard.IsKeyDown(Key.LeftAlt) &&
                !Keyboard.IsKeyDown(Key.RightAlt);

            // Sprawdzamy, czy jest wciśnięty Shift
            bool isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            // Dodajemy obsługę Ctrl + Shift + A => Zaznacz wszystko
            if (isCtrlPressed && isShiftPressed && e.Key == Key.A)
            {
                UserAnswerTextBox.SelectAll();
                e.Handled = true;
                return;
            }

            // Jeśli jest tylko Ctrl (bez Alt) to wpisujemy hiszpański znak
            if (isCtrlPressed)
            {
                switch (e.Key)
                {
                    case Key.A:
                        InsertCharacter('á');
                        e.Handled = true;
                        break;
                    case Key.E:
                        InsertCharacter('é');
                        e.Handled = true;
                        break;
                    case Key.I:
                        InsertCharacter('í');
                        e.Handled = true;
                        break;
                    case Key.O:
                        InsertCharacter('ó');
                        e.Handled = true;
                        break;
                    case Key.U:
                        InsertCharacter('ú');
                        e.Handled = true;
                        break;
                    case Key.N:
                        InsertCharacter('ñ');
                        e.Handled = true;
                        break;
                    case Key.OemQuestion:  // klawisz '/'
                        InsertCharacter('¿');
                        e.Handled = true;
                        break;
                    case Key.OemMinus:     // klawisz '-'
                        InsertCharacter('¡');
                        e.Handled = true;
                        break;
                }
            }
        }

        // Metoda pomocnicza do wstawiania znaku w miejscu kursora
        private void InsertCharacter(char character)
        {
            var textBox = UserAnswerTextBox;
            int selectionStart = textBox.SelectionStart;

            // Wstaw znak w miejscu kursora
            textBox.Text = textBox.Text.Insert(selectionStart, character.ToString());

            // Przesuń kursor za nowy znak
            textBox.SelectionStart = selectionStart + 1;
            textBox.SelectionLength = 0;
        }


    }
}