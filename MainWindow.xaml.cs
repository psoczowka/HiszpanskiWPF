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
            // Sprawdzamy, czy jest wciśnięty Ctrl + Alt lub RightAlt (AltGr)
            bool isCtrlAlt = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                             && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt));
            bool isAltGr = Keyboard.IsKeyDown(Key.RightAlt);

            // Jeżeli jest wciśnięty Ctrl + Alt lub AltGr
            if (isCtrlAlt || isAltGr)
            {
                TextBox textBox = sender as TextBox;
                int caretIndex = textBox.CaretIndex;

                switch (e.Key)
                {
                    case Key.N:
                        InsertSpecialCharacter(textBox, "ñ", caretIndex);
                        e.Handled = true;
                        break;
                    case Key.E:
                        InsertSpecialCharacter(textBox, "é", caretIndex);
                        e.Handled = true;
                        break;
                    case Key.I:
                        InsertSpecialCharacter(textBox, "í", caretIndex);
                        e.Handled = true;
                        break;
                    case Key.O:
                        InsertSpecialCharacter(textBox, "ó", caretIndex);
                        e.Handled = true;
                        break;
                    case Key.U:
                        InsertSpecialCharacter(textBox, "ú", caretIndex);
                        e.Handled = true;
                        break;
                    case Key.A:
                        InsertSpecialCharacter(textBox, "á", caretIndex);
                        e.Handled = true;
                        break;
                    case Key.OemQuestion: // ? → ¿
                        InsertSpecialCharacter(textBox, "¿", caretIndex);
                        e.Handled = true;
                        break;
                    case Key.Oem1: // ! → ¡ (Oem1 to klawisz z "!" na większości klawiatur)
                        InsertSpecialCharacter(textBox, "¡", caretIndex);
                        e.Handled = true;
                        break;
                }
            }
        }

        // Metoda pomocnicza do wstawiania znaku w miejscu kursora
        private void InsertSpecialCharacter(TextBox textBox, string character, int caretIndex)
        {
            textBox.Text = textBox.Text.Insert(caretIndex, character);
            textBox.CaretIndex = caretIndex + character.Length;
        }


    }
}