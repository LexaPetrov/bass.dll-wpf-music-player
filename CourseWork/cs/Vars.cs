using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWork
{
    /// <summary>
    /// Класс переменных:получение из конкретного пути, получение имени
    /// </summary>
    public static class Vars
    {      
        //Путь к исполняемому файлу
        public static string AppPath = AppDomain.CurrentDomain.BaseDirectory;

        //Список полных имён файлов
        public static List<string> Files = new List<string>();
        //Текущая позиция трека в плейлисте
        public static int CurrentTrackNumber;
        //Получаем имя файла: парсим по / чтобы получить красивое имя, а не имя полного пути
        public static string GetFileName(string file)
        {
            string[] tmp = file.Split('\\');
            return tmp[tmp.Length - 1];
        }       
    }
}
