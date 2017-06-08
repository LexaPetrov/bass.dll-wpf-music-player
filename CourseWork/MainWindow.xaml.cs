using System;
using System.Resources;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Un4seen.Bass.AddOn.Wma;
using System.IO;
using System.ComponentModel;
using System.Timers;
using TagLib;



namespace CourseWork
{
   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ////Инициализируем   библиотеку Bass с определенным состоянием инициализации InitBass и частотой дискретизации HZ 
            BassLike.InitBass(BassLike.HZ);   
        }
        OpenFileDialog ofd = new OpenFileDialog(); // Создаем переменную для работы с диалоговым окном
        DispatcherTimer dtimer = new DispatcherTimer(); // Сеременная для работы и инициализации таймера
        /// <summary>
        /// проверка формата файлов

        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool CheckFileName(string filename)
        {
            string[] ext = new string[] { ".mp3", ".m4a", ".wma", ".ogg", ".opus" };
            if (ext.Contains(System.IO.Path.GetExtension(filename)))
                return true;
            return false;
        }
        /// <summary>
        /// Нажатие на кнопку "добавить файлы"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnEject_Click(object sender, RoutedEventArgs e)
        {
            ofd.Multiselect = true;
            ofd.AddExtension = true;
            ofd.DefaultExt = "*.*";
            ofd.Filter = "All files (*.mp3; *.m4a; *.wma; *.ogg; *.opus) | *.mp3; *.m4a; *.wma; *.ogg; *.opus";
            ofd.FileOk += ofd_FileOk;
            ofd.ShowDialog();
            string fn = ofd.FileName;
            ofd.Multiselect = true;
            dtimer.Tick += dtimer_Tick;
            dtimer.Start(); 
        }
        /// <summary>
        /// Добавление названий композиций в окно плейлиста
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ofd_FileOk(object sender, CancelEventArgs e)
        {
            ofd.FileOk -= ofd_FileOk;
            string[] tmp = ofd.FileNames;
            if (CheckFileName(ofd.FileName))
            {
                for (int i = 0; i < tmp.Length; i++)
                {
                    Vars.Files.Add(tmp[i]);
                    TagModel TM = new TagModel(tmp[i]);
                    playlist.Items.Add(TM.Artist + " - " + TM.Title);
                }

            }
            else
            {
                e.Cancel = true;
                System.Windows.MessageBox.Show("Invalid format, please try again", "Error", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
        }
        /// <summary>
        /// Нажатие на кнопку "Плей"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            dtimer.Start();
            
            if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
            {
                labelNowPlaying.Content = "Now playing:";
                string current = Vars.Files[playlist.SelectedIndex];
                    Vars.CurrentTrackNumber = playlist.SelectedIndex;
                    BassLike.Play(current, BassLike.Volume);
                    labelLefttime.Content = TimeSpan.FromSeconds(BassLike.GetPosOfStream(BassLike.Stream)).ToString();
                    labelRightTime.Content = TimeSpan.FromSeconds(BassLike.GetTimeOfStream(BassLike.Stream)).ToString();
                    slTime.Maximum = BassLike.GetTimeOfStream(BassLike.Stream);
                    slTime.Value = BassLike.GetPosOfStream(BassLike.Stream);  
                    labelCurrentPlayingName.Content = Vars.GetFileName(current);
                //***************************************************************//
                
            
                try
                {
                    TagLib.File f = new TagLib.Mpeg.AudioFile(current);
                    TagLib.IPicture pic = f.Tag.Pictures[0];
                    var mStream = new MemoryStream(pic.Data.Data);
                    mStream.Seek(0, SeekOrigin.Begin);
                    BitmapImage bm = new BitmapImage();
                    bm.BeginInit();
                    bm.StreamSource = mStream;
                    bm.EndInit();
                    System.Windows.Controls.Image cover = new System.Windows.Controls.Image();
                    cover.Source = bm;
                    image.Source = bm;
                }
                catch
                {
                    var uri = new Uri("pack://application:,,,/Resources/nocover.png");
                    var img = new BitmapImage(uri);
                    image.Source = img;
                }
                //****************************************************************//
            }

        }
        /// <summary>
        /// Описано задание значений максимальной длины трека, минимально длины трека и значения ползунка
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtimer_Tick(object sender, EventArgs e)
        {
            if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
            {
                slTime.Minimum = 0;
                //Получаем значения посекундного измения времени трека при проигывании + вследствие этого движется ползунок
                slTime.Maximum = BassLike.GetTimeOfStream(BassLike.Stream);
                slTime.Value = BassLike.GetPosOfStream(BassLike.Stream);
                if (BassLike.ToNextTrack() == true)
                {
                    if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
                    {
                        BassLike.Stop();
                        btnNext_Click(this, new RoutedEventArgs());
                        btnPrev_Click(this, new RoutedEventArgs());
                        btnPlay_Click(this, new RoutedEventArgs());
                        slVol.Value = 100;
                        labelNowPlaying.Content = "Now playing:";
                        BassLike.Next();
                        labelLefttime.Content = TimeSpan.FromSeconds(BassLike.GetPosOfStream(BassLike.Stream)).ToString();
                        labelRightTime.Content = TimeSpan.FromSeconds(BassLike.GetTimeOfStream(BassLike.Stream)).ToString();
                        slTime.Maximum = BassLike.GetTimeOfStream(BassLike.Stream);
                        slTime.Value = BassLike.GetPosOfStream(BassLike.Stream);
                        try
                        {
                            if (Vars.Files.Count >= Vars.CurrentTrackNumber + 1)
                            {
                                ++playlist.SelectedIndex;
                                labelCurrentPlayingName.Content = Vars.GetFileName((Vars.Files[Vars.CurrentTrackNumber]));
                            }
                            if (Vars.CurrentTrackNumber == Vars.Files.Count)
                                labelCurrentPlayingName.Content = Vars.GetFileName(Vars.Files[playlist.SelectedIndex]);
                            if (Vars.CurrentTrackNumber == 0)
                            {
                                playlist.SelectedIndex = 0;
                                labelCurrentPlayingName.Content = Vars.GetFileName(Vars.Files[0]);
                            }
                            if (BassLike.isStopped)
                            {
                                labelLefttime.Content = null;
                                labelRightTime.Content = null;
                                labelCurrentPlayingName.Content = null;
                            }
                        }
                        catch
                        {
                            labelCurrentPlayingName.Content = null;
                        }                     
                    }


                    //*********************************************************//
                    string current = Vars.Files[playlist.SelectedIndex];
                    Vars.CurrentTrackNumber = playlist.SelectedIndex;
                    labelCurrentPlayingName.Content = Vars.GetFileName(current);
                    try
                    {
                        TagLib.File f = new TagLib.Mpeg.AudioFile(current);
                        TagLib.IPicture pic = f.Tag.Pictures[0];
                        var mStream = new MemoryStream(pic.Data.Data);
                        mStream.Seek(0, SeekOrigin.Begin);
                        BitmapImage bm = new BitmapImage();
                        bm.BeginInit();
                        bm.StreamSource = mStream;
                        bm.EndInit();
                        System.Windows.Controls.Image cover = new System.Windows.Controls.Image();
                        cover.Source = bm;
                        image.Source = bm;
                    }
                    catch
                    {
                        var uri = new Uri("pack://application:,,,/Resources/nocover.png");
                        var img = new BitmapImage(uri);
                        image.Source = img;
                    }
                    //***************************************************************************************//
                }
                if (BassLike.endPlaylist)
                {
                    btnStop_Click(this, new RoutedEventArgs());
                    playlist.SelectedIndex = Vars.CurrentTrackNumber = 0;
                    BassLike.endPlaylist = false;
                }
            }

        }
        /// <summary>
        /// Нажатие на кнопку "Стоп"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            BassLike.Stop();
            dtimer.Stop();
            slTime.Value = 0;
            labelLefttime.Content = "00:00:00";
            labelRightTime.Content = "00:00:00";
            labelNowPlaying.Content = " ";
            labelCurrentPlayingName.Content = " ";
        }
        /// <summary>
        /// При перемотке песни добавить текущую поз. воспроизведения в поток
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slTime_Scroll (object sender,ScrollEventArgs e)
        {
           BassLike.SetPosOfScroll(BassLike.Stream, (int)slTime.Value);
        }
        /// <summary>
        /// При изменении громкости добавить значение громкости в поток
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slVol_Scroll(object sender, ScrollEventArgs e)
        {    
            BassLike.SetVolumeToStream(BassLike.Stream,(int)slVol.Value);
        }
        /// <summary>
        /// Нажатие на кнопку "Пауза"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            BassLike.Pause();
        }
        /// <summary>
        /// Изменение позиции воспроизведения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            labelLefttime.Content = TimeSpan.FromSeconds(BassLike.GetPosOfStream(BassLike.Stream)).ToString();
            labelRightTime.Content = TimeSpan.FromSeconds(BassLike.GetTimeOfStream(BassLike.Stream)).ToString();
            BassLike.SetPosOfScroll(BassLike.Stream, (int)slTime.Value);  
        }
        /// <summary>
        /// Изменение звука
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slVol_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
           BassLike.SetVolumeToStream(BassLike.Stream, (int)slVol.Value);           
        }
        /// <summary>
        /// Открыть папку и добавить файлы с неё
        /// mp3 только, через "добавить файл" можно все доступные разрешенные форматы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_EjectFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            DialogResult result = dlg.ShowDialog();
            dlg.ShowNewFolderButton = false;
            if (System.Windows.Forms.DialogResult.OK == result)
            {
                dtimer.Tick += new EventHandler(dtimer_Tick);             
                dtimer.Start();
                foreach (string currentFile in System.IO.Directory.GetFiles(dlg.SelectedPath, "*.mp3", SearchOption.AllDirectories))

                {            
                        Vars.Files.Add(currentFile);
                        TagModel TM = new TagModel(currentFile);
                        playlist.Items.Add(TM.Artist + " - " + TM.Title);
                }
            }

        }
        /// <summary>
        /// Нажатие на кнопку "Плей"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click(object sender, MouseButtonEventArgs e)
        {

            if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
            {
                dtimer.Start();
                labelNowPlaying.Content = "Now playing:";
                string current = Vars.Files[playlist.SelectedIndex];
                Vars.CurrentTrackNumber = playlist.SelectedIndex;
                BassLike.Play(current, BassLike.Volume);
                labelLefttime.Content = TimeSpan.FromSeconds(BassLike.GetPosOfStream(BassLike.Stream)).ToString();
                labelRightTime.Content = TimeSpan.FromSeconds(BassLike.GetTimeOfStream(BassLike.Stream)).ToString();
                slTime.Maximum = BassLike.GetTimeOfStream(BassLike.Stream);
                slTime.Value = BassLike.GetPosOfStream(BassLike.Stream);
                labelCurrentPlayingName.Content = Vars.GetFileName(current);
                //************************************************************************//
                try
                {
                    TagLib.File f = new TagLib.Mpeg.AudioFile(current);
                    TagLib.IPicture pic = f.Tag.Pictures[0];
                    var mStream = new MemoryStream(pic.Data.Data);
                    mStream.Seek(0, SeekOrigin.Begin);
                    BitmapImage bm = new BitmapImage();
                    bm.BeginInit();
                    bm.StreamSource = mStream;
                    bm.EndInit();
                    System.Windows.Controls.Image cover = new System.Windows.Controls.Image();
                    cover.Source = bm;
                    image.Source = bm;
                }
                catch
                {
                    var uri = new Uri("pack://application:,,,/Resources/nocover.png");
                    var img = new BitmapImage(uri);
                    image.Source = img; ;
                }
                //***************************************************************************************//
            }
        }
        /// <summary>
        /// Нажатие на кнопку "Следующий трек"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {

            if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
            {
                labelNowPlaying.Content = "Now playing:";
                BassLike.Next();
                slVol.Value = 100;
                labelLefttime.Content = TimeSpan.FromSeconds(BassLike.GetPosOfStream(BassLike.Stream)).ToString();
                labelRightTime.Content = TimeSpan.FromSeconds(BassLike.GetTimeOfStream(BassLike.Stream)).ToString();
                slTime.Maximum = BassLike.GetTimeOfStream(BassLike.Stream);
                slTime.Value = BassLike.GetPosOfStream(BassLike.Stream);
                try
                {
                    if (Vars.Files.Count >= Vars.CurrentTrackNumber + 1)
                    {
                        ++playlist.SelectedIndex;
                        labelCurrentPlayingName.Content = Vars.GetFileName((Vars.Files[Vars.CurrentTrackNumber]));
                    }
                    else if (Vars.CurrentTrackNumber == Vars.Files.Count)
                        labelCurrentPlayingName.Content = Vars.GetFileName(Vars.Files[playlist.SelectedIndex]);
                    if (BassLike.isStopped)
                    {
                        labelLefttime.Content = null;
                        labelRightTime.Content = null;
                        labelCurrentPlayingName.Content = null;
                    }
                }
                catch
                {
                    labelCurrentPlayingName.Content = null;
                }
                //***************************************************************************************//
                string current = Vars.Files[playlist.SelectedIndex];
                try
                {
                    TagLib.File f = new TagLib.Mpeg.AudioFile(current);
                    TagLib.IPicture pic = f.Tag.Pictures[0];
                    var mStream = new MemoryStream(pic.Data.Data);
                    mStream.Seek(0, SeekOrigin.Begin);
                    BitmapImage bm = new BitmapImage();
                    bm.BeginInit();
                    bm.StreamSource = mStream;
                    bm.EndInit();
                    System.Windows.Controls.Image cover = new System.Windows.Controls.Image();
                    cover.Source = bm;
                    image.Source = bm;
                }
                catch
                {
                    var uri = new Uri("pack://application:,,,/Resources/nocover.png");
                    var img = new BitmapImage(uri);
                    image.Source = img;
                }
                //***************************************************************************************//
            }
        }
        /// <summary>
        /// Нажатие на кнопку "Предыдущий трек"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
            {
                labelNowPlaying.Content = "Now playing:";
                BassLike.Prev();
                slVol.Value = 100;
                labelLefttime.Content = TimeSpan.FromSeconds(BassLike.GetPosOfStream(BassLike.Stream)).ToString();
                labelRightTime.Content = TimeSpan.FromSeconds(BassLike.GetTimeOfStream(BassLike.Stream)).ToString();
                slTime.Maximum = BassLike.GetTimeOfStream(BassLike.Stream);
                slTime.Value = BassLike.GetPosOfStream(BassLike.Stream);
                if ((Vars.CurrentTrackNumber - 1) >= 0)
                {
                    --playlist.SelectedIndex;
                    labelCurrentPlayingName.Content = Vars.GetFileName((Vars.Files[Vars.CurrentTrackNumber]));
                    if (Vars.CurrentTrackNumber == 0)
                    {
                        playlist.SelectedIndex = 0;
                    }
                }
                else if (Vars.CurrentTrackNumber == 0)
                {
                    playlist.SelectedIndex = 0;
                    labelCurrentPlayingName.Content = Vars.GetFileName(Vars.Files[0]);
                }
                if (BassLike.isStopped)
                {
                    labelLefttime.Content = null;
                    labelRightTime.Content = null;
                    labelCurrentPlayingName.Content = null;
                }

        //******************************************************************************************//
                string current = Vars.Files[playlist.SelectedIndex];
                try
                {
                    TagLib.File f = new TagLib.Mpeg.AudioFile(current);
                    TagLib.IPicture pic = f.Tag.Pictures[0];
                    var mStream = new MemoryStream(pic.Data.Data);
                    mStream.Seek(0, SeekOrigin.Begin);
                    BitmapImage bm = new BitmapImage();
                    bm.BeginInit();
                    bm.StreamSource = mStream;
                    bm.EndInit();
                    System.Windows.Controls.Image cover = new System.Windows.Controls.Image();
                    cover.Source = bm;
                    image.Source = bm;
                }
                catch
                {
                    var uri = new Uri("pack://application:,,,/Resources/nocover.png");
                    var img = new BitmapImage(uri);
                    image.Source = img;
                }
            }
                //***************************************************************************************//
            
        }
        /// <summary>
        /// Нажатие на кнопку "Удалить трек из плейлиста"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if ((playlist.Items.Count != 0) && (playlist.SelectedIndex != -1))
            {
                BassLike.Stop();
                Vars.CurrentTrackNumber = playlist.SelectedIndex;
                Vars.Files.Remove(Vars.Files[playlist.SelectedIndex]);
                playlist.Items.RemoveAt(playlist.SelectedIndex);
                var uri = new Uri("pack://application:,,,/Resources/nocover.png");
                var img = new BitmapImage(uri);
                image.Source = img;
                labelLefttime.Content = "00:00:00";
                labelRightTime.Content = "00:00:00";
                labelNowPlaying.Content = null;
                labelCurrentPlayingName.Content = null;
            }
        }
        /// <summary>
        /// Нажатие на кнопку "О программе"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnContatcs_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Petrov Alexey: MusicPlayer v.1.0. All rights received (c) 2016-20**. For contacts: telegram.me/killing4fun & al1111997@yandex.ru ",
                "INFO", MessageBoxButton.OK, MessageBoxImage.Information);   
         }
        /// <summary>
        /// Запрет перемещения в листбоксе (плейлисте) стрелочками
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listbox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
    }
  
