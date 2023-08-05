using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppConvertStringToDouble
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string record = Record.Text;
            double result;
            try
            {
                result = ConvertStringToDouble(record);
            }
            catch (FormatException)
            {
                Result.Text = "Входная строка имела неверный формат";
                Received.Text = string.Empty;
                return;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            Result.Text = "Преобразование успешно!";
            Received.Text = $"Полученное число: {result}"; //при выводе double на экран
                                                           //вместо разделителя '.' отображается ','
                                                           //но на деле в системе все нормально, и там '.'
        }
        private double ConvertStringToDouble(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                List<char> chars = value.ToCharArray().ToList();

                //поиск количества разделителей
                //(допускается: один разделитель '.',
                //или несколько ',' но первый по входу превратится в '.'
                // допускается число 13.456,654,678 - для обозначения разрядов
                int count_separator_dot = value.Split('.').Length - 1;
                int count_separator_virg = value.Split(',').Length - 1;

                if (count_separator_dot > 1)
                {
                    throw new FormatException();
                }
                if (count_separator_dot == 0) 
                {
                    if (count_separator_virg > 0) //если есть запятые, первая должна превратится в точку, остальные стереть
                    {
                        //первая запятая
                        int index_first_virg = chars.IndexOf(','); 
                        chars[index_first_virg] = '.'; 
                    }
                }
                //удаление лишних запятых
                for (int i = 0; i < chars.Count; i++)
                {
                    if (chars[i] == ',')
                    {
                        chars.RemoveAt(i); 
                    }
                }
                //проверка на отрицательное значение
                bool negative = false;
                //подсчёт кол-ва минусов и их удаление
                int count_minus = 0;
                
                for (int i = 0; i <= chars.Count-1; i++)
                {
                    if (chars[i] == '-')
                    {

                        count_minus++;
                        //если минус стоит в начале и он один, значит число отрицательное
                        if (count_minus == 1 && i == 0)
                        {
                            negative = true;
                        }
                        else
                        {
                            //если он один, но не в начале, значит где то в середине или в конце, а так нельзя
                            //больше одного минуса тоже быть не может
                            throw new FormatException();
                        }
                        chars.RemoveAt(i);
                    }
                }

                //проверка, что все символы являются цифрами (одну точку можно)
                //изначально хотел все запихнуть в цикл выше, чтоб меньше циклов делать, но пусть будет разделение в коде (так ведь правильно)
                for (int i = 0; i < chars.Count; i++)
                {
                    if (!Char.IsNumber(chars[i]) && chars[i] != '.')
                    {
                        throw new FormatException();
                    }
                }
                //теперь есть отформатированный коллекция символов, которые представляют из себя 1 3 . 3 2 2
                //это надо переделать в double
                //сначала разбить на две половины, через точку, потом каждый набор цифр
                int index_dot = chars.IndexOf('.');
                //если не нашел разделитель, то индекс -1

                double ret = 0;

                int digit = 1; //разрядность
                if (index_dot == -1)
                {
                    //целое число
                    for (int i = chars.Count-1; i >= 0; i--)
                    {
                        ret = ret + digit * CharToDouble(chars[i]);
                        digit = digit * 10;
                    }
                }
                else if (index_dot > 0)
                {
                    //дробное число
                    for (int i = index_dot - 1; i >= 0; i--) //целая часть
                    {
                        ret = ret + digit * CharToDouble(chars[i]);
                        digit = digit * 10;
                    }
                    digit = 10;
                    for (int i = index_dot + 1; i < chars.Count; i++) //дробная часть
                    {
                        ret = ret + CharToDouble(chars[i]) / digit;
                        digit = digit * 10;
                    }
                }
                else
                {
                    //есть разделитель есть, но он в начале
                    throw new FormatException();
                }
                if (negative)
                {
                    ret *= -1; 
                }
                return ret;
            }
            return 0.0;
        }

        private double CharToDouble(char c)
        {
            return c - '0';
        }

    } 
}
