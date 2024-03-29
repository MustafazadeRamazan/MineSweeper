﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Minesweeper_WPF
{
    public partial class MainWindow : Window
    {
        #region Variables
        private int difficultylevel = 1;

        private int _xFieldsCount = 10;
        private int _yFieldsCount = 8;
        private int _fieldSize = 50;

        private int _bombCount = 10;
        private int _windowHeight;
        private int _windowWidth;

        private int _timer = 0;
        private int _flagsPlaced = 10;

        private List<Image> tempImages = new List<Image>();
        private List<string> _bombIndexes = new List<string>();
        private List<Image> openedImages = new List<Image>();

        DispatcherTimer timer = new DispatcherTimer();
        private bool _playingGame = false;
        #endregion

        #region Game Methods

        public MainWindow()
        {
            InitializeComponent();
            InitializateSetup();
        }
        public MainWindow(int difficultySelect)
        {
            InitializeComponent();
            LevelSetup(difficultySelect);
            InitializateSetup();
            difficultylevel = difficultySelect;

            if (difficultylevel == 1)
            {
                btnDifficulty.Background = Brushes.Green;
            }

            else if (difficultylevel == 2)
            {
                btnDifficulty.Background = Brushes.Yellow;
            }

            else
            {
                btnDifficulty.Background = Brushes.Red;
            }
        }

        private void LevelSetup(int difficultyNumber)
        {
            if (difficultyNumber == 1)
            {
                _xFieldsCount = 10;
                _yFieldsCount = 8;
                _fieldSize = 50;
                _bombCount = 10;
                _flagsPlaced = 10;
                tbDifficulty.Text = "EASY";
            }

            else if (difficultyNumber == 2)
            {
                _xFieldsCount = 18;
                _yFieldsCount = 14;
                _fieldSize = 40;
                _bombCount = 40;
                _flagsPlaced = 40;
                tbDifficulty.Text = "NORMAL";
            }

            else
            {
                _xFieldsCount = 24;
                _yFieldsCount = 20;
                _fieldSize = 35;
                _bombCount = 100;
                _flagsPlaced = 100;
                tbDifficulty.Text = "HARD";
            }
        }

        private void InitializateSetup()
        {
            _windowHeight = _yFieldsCount * _fieldSize + 33 + 70;
            _windowWidth = _xFieldsCount * _fieldSize;
            this.Width = _windowWidth;
            this.Height = _windowHeight;
            lbFlagsAvailable.Content = "Flags: " + _flagsPlaced;

            generateFields(_xFieldsCount, _yFieldsCount);
            generateBombs(_xFieldsCount, _yFieldsCount, _bombCount);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            _timer++;
            lbTime.Content = "Time: " + _timer + " second";
        }

        private void generateFields(int x, int y)
        {
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    Image img = new Image()
                    {
                        Name = "hasField",
                        Width = _fieldSize,
                        Height = _fieldSize,
                        Tag = j + "," + i,
                        Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"/Images/0.png"))
                    };
                    img.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(checkField);
                    img.PreviewMouseRightButtonDown += new MouseButtonEventHandler(setFlag);
                    mainGrid.Children.Add(img);
                }
            }
        }

        private void checkArround(Image img)
        {
            if (!tempImages.Contains(img))
            {
                tempImages.Add(img);
            }

            if (!openedImages.Contains(img))
            {
                openedImages.Add(img);
            }

            bool endReveal = false;
            int animationCounter = 1;

            while (!endReveal)
            {
                for (int i = 0; i < tempImages.Count; i++)
                {
                    string[] tempString = tempImages[i].Tag.ToString().Split(',');
                    int tempxValue = Int32.Parse(tempString[0]);
                    int tempyValue = Int32.Parse(tempString[1]);

                    foreach (Image newImg in mainGrid.Children)
                    {
                        if (newImg.Tag.ToString().Equals((tempxValue - 1) + "," + (tempyValue - 1)) && tempxValue > 0 && tempyValue > 0 && !openedImages.Contains(newImg)) { tempImages.Add(newImg); openedImages.Add(newImg); }
                        else if (newImg.Tag.ToString().Equals(tempxValue + "," + (tempyValue - 1)) && tempyValue > 0 && !openedImages.Contains(newImg)) { tempImages.Add(newImg); openedImages.Add(newImg); }
                        else if (newImg.Tag.ToString().Equals((tempxValue + 1) + "," + (tempyValue - 1)) && tempxValue < _xFieldsCount && tempyValue > 0 && !openedImages.Contains(newImg)) { tempImages.Add(newImg); openedImages.Add(newImg); }
                        else if (newImg.Tag.ToString().Equals((tempxValue - 1) + "," + tempyValue) && tempxValue > 0 && !openedImages.Contains(newImg)) { tempImages.Add(newImg); openedImages.Add(newImg); }
                        else if (newImg.Tag.ToString().Equals((tempxValue + 1) + "," + tempyValue) && tempxValue < _xFieldsCount && !openedImages.Contains(newImg)) { tempImages.Add(newImg); openedImages.Add(newImg); }
                        else if (newImg.Tag.ToString().Equals((tempxValue - 1) + "," + (tempyValue + 1)) && tempxValue > 0 && tempyValue < _yFieldsCount && !openedImages.Contains(newImg)) { tempImages.Add(newImg); openedImages.Add(newImg); }
                        else if (newImg.Tag.ToString().Equals(tempxValue + "," + (tempyValue + 1)) && tempyValue < _yFieldsCount && !openedImages.Contains(newImg)) { tempImages.Add(newImg); openedImages.Add(newImg); }
                        else if (newImg.Tag.ToString().Equals((tempxValue + 1) + "," + (tempyValue + 1)) && tempxValue < _xFieldsCount && tempyValue < _yFieldsCount && !openedImages.Contains(newImg)) { tempImages.Add(newImg); openedImages.Add(newImg); }
                    }
                    tempImages.Remove(tempImages[i]);
                    break;
                }

                for (int i = tempImages.Count - 1; i >= 0; i--)
                {
                    tempImages[i].Name = "";
                    checkFlagCount();

                    revealAfterTimerEnd(tempImages[i], tempImages);
                    animationCounter++;
                }
                if (tempImages.Count == 0) endReveal = true;
            }
        }

        private void checkFieldByIndexAndImage(Image img)
        {
            if (_playingGame)
            {
                img.Name = "";
                checkFlagCount();

                if (_bombIndexes.Contains(img.Tag.ToString()))
                {
                    img.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"/Images/Bomb.png"));
                    for (int i = 0; i < _yFieldsCount; i++)
                    {
                        for (int j = 0; j < _xFieldsCount; j++)
                        {
                            for (int k = 0; k < _bombCount; k++)
                            {
                                if (_bombIndexes[k].Equals(j + "," + i))
                                {
                                    foreach (Image spcificImage in mainGrid.Children)
                                    {
                                        if (spcificImage.Tag.ToString().Equals(j + "," + i))
                                        {
                                            spcificImage.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"/Images/Bomb.png"));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    MessageBox.Show("Game Over!", "Loser", MessageBoxButton.OK);
                    _playingGame = false;
                    timer.Stop();
                    btnDifficulty.Visibility = Visibility.Visible;
                }
                else
                {
                    if (checkAround(img.Tag.ToString()) != 0)
                    {
                        img.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"/Images/" + checkAround(img.Tag.ToString()) + ".png"));
                    }
                    else
                    {
                        img.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"/Images/bos.png"));
                        checkArround(img);
                    }
                }
                checkIfUserWon();
            }
        }

        private void generateBombs(int x, int y, int bomb)
        {
            while(_bombIndexes.Count < bomb)
            {
                Random r = new Random();
                int xValue = r.Next(0, x);
                int yValue = r.Next(0, y);

                if (!_bombIndexes.Contains(xValue + "," + yValue))
                {
                    _bombIndexes.Add(xValue + "," + yValue);
                }
            }
        }

        private void checkField(object sender, MouseButtonEventArgs e)
        {
            checkFieldByIndexAndImage(sender as Image);
            checkIfUserWon();
        }

        private void setFlag(object sender, MouseButtonEventArgs e)
        {
            if (_playingGame)
            {
                Image imgToSwapField = new Image() { Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"/Images/0.png")) };
                Image imgToSwapFlag = new Image() { Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"/Images/Flag.png")) };

                if ((sender as Image).Name == "hasFlag")
                {
                    _flagsPlaced++;
                    (sender as Image).Source = imgToSwapField.Source;
                    (sender as Image).Name = "hasField";
                }
                else if ((sender as Image).Name == "hasField" && _flagsPlaced > 0)
                {
                    _flagsPlaced--;
                    (sender as Image).Source = imgToSwapFlag.Source;
                    (sender as Image).Name = "hasFlag";
                }
                lbFlagsAvailable.Content = "Flags: " + _flagsPlaced;
            }
        }

        private void revealAfterTimerEnd(Image image, List<Image> tempImages)
        {
            if (checkAround(image.Tag.ToString()) != 0)
            {
                foreach (Image newImg in mainGrid.Children)
                {
                    if (newImg.Tag == image.Tag) newImg.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"/Images/" + checkAround(image.Tag.ToString()) + ".png"));
                }
                tempImages.Remove(image);
            }
            else
            {
                foreach (Image newImg in mainGrid.Children)
                {
                    if (newImg.Tag == image.Tag) newImg.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"/Images/bos.png"));
                }
            }
            DoubleAnimation ani = new DoubleAnimation(0.5, 1, TimeSpan.FromSeconds(0.1));
            image.BeginAnimation(OpacityProperty, ani);
        }

        private void checkFlagCount()
        {
            int flagCounter = 0;
            foreach (Image images in mainGrid.Children)
            {
                if (images.Name == "hasFlag") flagCounter++;
            }
            _flagsPlaced = _bombCount - flagCounter;
            lbFlagsAvailable.Content = "Flags: " + _flagsPlaced;
        }

        private void checkIfUserWon()
        {
            int counter = 0;
            foreach (Image img in mainGrid.Children)
            {
                if(img.Name == "") counter++;

                if (counter == _xFieldsCount * _yFieldsCount - _bombCount)
                {
                    timer.Stop();
                    _playingGame = false;
                    mainGrid.Opacity = 0.2;
                    gridResult.Visibility = Visibility.Visible;
                    tbWin.Text = "You Win!\nFinished in " + _timer + "second";
                }
            }
        }

        private int checkAround(string str)
        {
            string[] arrayNewString = str.Split(',');
            int xValue = Int32.Parse(arrayNewString[0]);
            int yValue = Int32.Parse(arrayNewString[1]);

            int count = 0;
            for (int i = 0; i < _bombCount; i++)
            {
                if (_bombIndexes[i].Equals((xValue + 1) + "," + (yValue - 1)) && xValue < _xFieldsCount && yValue > 0) count++;
                else if (_bombIndexes[i].Equals((xValue + 1) + "," + yValue) && xValue < _xFieldsCount) count++;
                else if (_bombIndexes[i].Equals((xValue + 1) + "," + (yValue + 1)) && xValue < _xFieldsCount && yValue < _yFieldsCount) count++;
                else if (_bombIndexes[i].Equals(xValue + "," + (yValue - 1)) && yValue > 0) count++;
                else if (_bombIndexes[i].Equals(xValue + "," + (yValue + 1)) && yValue < _yFieldsCount) count++;
                else if (_bombIndexes[i].Equals((xValue - 1) + "," + (yValue - 1)) && xValue > 0 && yValue > 0) count++;
                else if (_bombIndexes[i].Equals((xValue - 1) + "," + yValue) && xValue > 0) count++;
                else if (_bombIndexes[i].Equals((xValue - 1) + "," + (yValue + 1)) && xValue > 0 && yValue < _yFieldsCount) count++;
            }
            return count;
        }
        #endregion

        #region Button Methods

        private void btnExit_MouseEnter(object sender, MouseEventArgs e)
        {
            btnExit.Background = Brushes.Red;
        }
        private void btnExit_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExit.Background = new SolidColorBrush(Color.FromArgb(0xFF, 29, 29, 29));
        }
        private void btnExit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        private void dragFrameFunction(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            btnDifficulty.Visibility = Visibility.Visible;
            btnStart.Visibility = Visibility.Visible;
            btnReset.Visibility = Visibility.Collapsed;
            mainGrid.Children.Clear();
            _bombIndexes.Clear();
            generateFields(_xFieldsCount, _yFieldsCount);
            generateBombs(_xFieldsCount, _yFieldsCount, _bombCount);
            _playingGame = false;
            lbTime.Content = "Time: 0 second";
            lbFlagsAvailable.Content = "Flags: " + _bombCount;
            gridResult.Visibility = Visibility.Collapsed;
            _flagsPlaced = _bombCount;
            mainGrid.Opacity = 1;
            _timer = 0;
        }
        private void btnDifficulty_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow(difficultylevel++ == 3 ? 1 : difficultylevel++);

            mw.Show();
            mw.Top = this.Top;
            mw.Left = this.Left;
            this.Close();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnDifficulty.Visibility = Visibility.Collapsed;
            btnStart.Visibility = Visibility.Collapsed;
            btnReset.Visibility = Visibility.Visible;
            timer.Stop();
            mainGrid.Children.Clear();
            _bombIndexes.Clear();
            generateFields(_xFieldsCount, _yFieldsCount);
            generateBombs(_xFieldsCount, _yFieldsCount, _bombCount);
            _playingGame = true;
            lbTime.Content = "Time: 0 second";
            lbFlagsAvailable.Content = "Flags: " + _bombCount;
            gridResult.Visibility = Visibility.Collapsed;
            _flagsPlaced = _bombCount;
            mainGrid.Opacity = 1;
            _timer = 0;
            timer.Start();
        }
        #endregion

    }
}