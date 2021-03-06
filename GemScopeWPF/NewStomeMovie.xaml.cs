﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using GemScopeWPF.Repository;
using GemScopeWPF.UI;
using GemScopeWPF.WebcamFacade;
using System.Text.RegularExpressions;
using CAVEditLib;

namespace GemScopeWPF
{
    /// <summary>
    /// Interaction logic for NewStomeMovie.xaml
    /// </summary>
    public partial class NewStomeMovie : Window
    {

        public string TempFileName { get; set; }
        public bool EditMode { get; set; }
        public Stone CurrentStone { get; set; }
        public string FolderUponCaptureEvent { get; set; }
        public NewStomeMovie()
        {
            InitializeComponent();
            EditMode = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            if (CurrentStone != null)
            {
                LoadMovieDetailsByStone(CurrentStone);

                //load iamge

                this.mediaPlayer.Source = new Uri(CurrentStone.FullFilePath, UriKind.Absolute);
                this.mediaPlayer.LoadedBehavior = WPFMediaKit.DirectShow.MediaPlayers.MediaState.Play;
              


            }
            else
            {
                this.mediaPlayer.Source = new Uri(this.TempFileName, UriKind.Absolute);
                this.mediaPlayer.LoadedBehavior = WPFMediaKit.DirectShow.MediaPlayers.MediaState.Play;
            }



        }

        private void LoadMovieDetailsByStone(Stone currentStone)
        {
            foreach (StackPanel panel in this.InfoPartsInputsContainer.Children)
            {
                if (panel.Children.OfType<TextBox>().SingleOrDefault() != null)
                {
                    string tag = (string) panel.Children.OfType<TextBox>().SingleOrDefault().Tag;
                    var value = currentStone.InfoList.Where(m => m.Title == tag).SingleOrDefault();
                    if (value != null)
                    {
                        panel.Children.OfType<TextBox>().SingleOrDefault().Text = value.Value;
                    }
                }
                else if (panel.Children.OfType<ComboBox>().SingleOrDefault() != null)
                {
                    string tag = (string) panel.Children.OfType<ComboBox>().SingleOrDefault().Tag;
                    var value = currentStone.InfoList.Where(m => m.Title == tag).SingleOrDefault();

                    if (value != null)
                    {
                        ComboBox combo = panel.Children.OfType<ComboBox>().SingleOrDefault();

                        combo.SelectedItem =
                            combo.Items.OfType<ComboBoxItem>().Where(m => (string) m.Tag == value.Value).Single();
                        //  = combo.Items.Count - 1;
                        //  combo.Items.OfType<ComboBoxItem>().Where(m => m.Tag == value.Value).Single().IsSelected = true;
                        //  combo.SelectedIndex = combo.Items.IndexOf(combo.Items.OfType<ComboBoxItem>().Where(m => m.Tag == value.Value).Single());
                    }
                }

                // string tag = (string)panel.Children.OfType<TextBox>().SingleOrDefault().Tag;

                //  var value = CurrentStone.InfoList.Where(m=> m.Title == tag).SingleOrDefault() ;

                //  if (value != null)
                // {
                //     panel.Children.OfType<TextBox>().SingleOrDefault().Text = value.Value;
                //}
            }
        }

        //   private string mLogPath = "C:\\Log\\Log.txt";
        private const string LIBAV_PATH = "LibAV";

        private void SaveDiamond_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                StonesRepository rep = new StonesRepository(FolderUponCaptureEvent);

                //Create the infoparts from input controls

                if (this.ValidateUserInput() == false)
                {
                    MessageBox.Show("Filename,Stonetype,Stone Weight,Clarity and color are mendatory fields...");
                    return;
                }

                string filename = Path.Combine(FolderUponCaptureEvent, this.Filename.Text);
                filename = Path.ChangeExtension(filename, "mp4");

                if (rep.IsStoneExists(filename) && this.EditMode == false)
                {
                    MessageBoxResult result = MessageBox.Show("The filename already exists, do you want to override the existing file", "File exists", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                }
            
                if (!String.IsNullOrWhiteSpace(TempFileName))
                {
                    ICAVConverter converter = new CAVConverter();
                    

                    converter.SetLicenseKey("orna@diamondsview.com", "62A80CE3A6DB3F755C8C7478D44332E5928296AC08E61FCCD052D464049EDDEFCE62B2ED069F1396AC27F7DA120E6A3FDE982ACA8B49A4E8735552303198E03250EFE9D36E9E5349D2E289FF18D6C919CAB81199B4B24B13ABDF70CBB53605C844B1ED2C4164D08B1B4392C526C3D49E4100C1399C052C986BA57392042AC468BF0DDFD7BA5B12AFC4F2FFC21F9DF83D75C054DC6DBC198B091AD0E8AFD49D7A8CBA1E6B1BEA6BE3E0A3B41DA51B79E0678A7B675D3183618229AF2D50650A8505E9EA106E8507156E53ECA07973D883152F3CF4A75EBA57576B50A56117E2B129C5734150D552B7519D28AA7368895C740444BFEC403C041F4BA207F1258786");

                    converter.LoadAVLib(AppDomain.CurrentDomain.BaseDirectory + LIBAV_PATH);

                  

                    converter.OutputOptions = new COutputOptions();
                    converter.OutputOptions.FrameSize = "640x480";
                    converter.OutputOptions.VideoBitrate = Convert.ToInt32(((ComboBoxItem)cmb_quality.SelectedItem).Tag);
                    converter.OutputOptions.DisableAudio = true;
                    converter.OutputOptions.FrameRate = "30";

                    //Set output video bitrate

            
      

                   // converter.LogPath = @"c:\log.txt";
                   //  converter.LogLevel = CLogLevel.cllDebug;

                   

                    try
                    {
                        converter.AddTask(TempFileName, filename);
                        converter.StartAndWait();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        throw;
                    }



                    // File.Move(TempFileName, filename);
                }


                List<StoneInfoPart> infoparts = new List<StoneInfoPart>();

                foreach (StackPanel panel in this.InfoPartsInputsContainer.Children)
                {
                    StoneInfoPart infopart = new StoneInfoPart();
                    if (panel.Children.OfType<TextBox>().SingleOrDefault() != null)
                    {
                        infopart.Title = (string)panel.Children.OfType<TextBox>().SingleOrDefault().Tag;
                        infopart.Value = panel.Children.OfType<TextBox>().SingleOrDefault().Text;
                        infopart.TitleForReport = (string)panel.Children.OfType<Label>().SingleOrDefault().Content;
                    }
                    else if (panel.Children.OfType<ComboBox>().SingleOrDefault() != null)
                    {
                        infopart.Title = (string)panel.Children.OfType<ComboBox>().SingleOrDefault().Tag;
                        infopart.Value = (string)((ComboBoxItem)panel.Children.OfType<ComboBox>().SingleOrDefault().SelectedItem).Tag;
                        infopart.TitleForReport = (string)panel.Children.OfType<Label>().SingleOrDefault().Content;
                    }


                    infoparts.Add(infopart);

                }

                if (this.EditMode)
                {
                    rep.UpdateStone(filename, infoparts);
                }
                else
                {
                    rep.CreateANewStoneMovie(filename, infoparts);
                }

                //WebCam webcam = WebCam.GetInstance();
                // webcam.Start();

                StonesView.RefreshView();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show(ex.StackTrace);
                throw;
            }


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.mediaPlayer.Stop();
            if (!String.IsNullOrWhiteSpace(this.TempFileName))
            {
                File.Delete(this.TempFileName);
            }
            
            this.Close();
        }

        private bool ValidateUserInput()
        {
            bool isValid = true;

            string filename = this.Filename.Text;
            int istonetype = this.StoneType.SelectedIndex;

            string caratweight = this.CaratWeight.Text;

            int icolor = this.StoneColor.SelectedIndex;
            int iclarity = this.StoneClarity.SelectedIndex;

            //filename required

            if (String.IsNullOrWhiteSpace(filename))
            {
                isValid = false;
            }

            Regex r = new Regex(@"[^/?*:;{}\\]+");
            isValid = r.IsMatch(filename);

            if (istonetype == 1)
            {
                isValid = false;
            }

            r = new Regex(@"^\d{0,8}(\.\d{1,8})?$");
            isValid = r.IsMatch(caratweight);

            if (icolor == 0)
            {
                isValid = false;
            }

            if (iclarity == 0)
            {
                isValid = false;
            }

            return isValid;


        }

        private void ImportDetails(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg"; // Default file extension
            dlg.Filter = "Diamond image (.jpg)|*.jpg|Movies|*.mp4"; // Filter files by extension

            // Show open file dialog box
            var result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                var fullFileName = dlg.FileName;
                var filename = Path.GetFileName(fullFileName);
                var folder = Path.GetDirectoryName(fullFileName);
                var stoneRepository = new StonesRepository(folder);

                if (stoneRepository.IsStoneExistsInRep(filename))
                {
                    var importedStone = stoneRepository.LoadStoneByFilenameInCurrentFolder(filename);
                    LoadMovieDetailsByStone(importedStone);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.mediaPlayer.Play();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.mediaPlayer.Pause();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.mediaPlayer.Stop();
            this.mediaPlayer.MediaPosition = 0;
        }
    }
}
