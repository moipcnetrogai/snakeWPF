using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using System.Linq.Expressions;

namespace wpfsnake{
    /// <summary>
    /// Логика взаимодействия для GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window{
        public ObservableCollection<SnakeHighscore> HighscoreList{
            get; set;
        } = new ObservableCollection<SnakeHighscore>();
        private Random rnd = new Random();
        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();
        const int SnakeSquareSize = 20;
        const int SnakeStartLength = 3;
        const int SnakeStartSpeed = 300;
        const int SnakeSpeedThreshold = 100;
        const int MaxHighscoreListEntryCount = 5;
        private UIElement snakeFood = null;
        private SolidColorBrush foodBrush = Brushes.Red;
        private SolidColorBrush snakeBodyBrush = Brushes.Green;
        private SolidColorBrush snakeHeadBrush = Brushes.YellowGreen;
        private SolidColorBrush mineBrush = Brushes.Black;
        private List<SnakePart> snakeParts = new List<SnakePart>();
        private List<Mine> mines = new List<Mine>();
        public enum SnakeDirection { Left, Right, Up, Down };
        private SnakeDirection snakeDirection = SnakeDirection.Right;
        private int snakeLength;
        private int currentScore = 0;
        public GameWindow(){
            InitializeComponent();
            gameTickTimer.Tick += GameTickTimer_Tick;
            LoadHighscoreList();
        }
        private void Window_ContentRendered(object sender, EventArgs e){
            DrawGameArea();
        }
        private void DrawGameArea(){
            bool doneDrawingBackground = false;
            int nextX = 0, nextY = 0;
            int rowCounter = 0;
            bool nextIsOdd = false;
            while (doneDrawingBackground == false){
                Rectangle rect = new Rectangle{
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = nextIsOdd ? Brushes.RoyalBlue : Brushes.RoyalBlue
                };
                GameArea.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);
                nextIsOdd = !nextIsOdd;
                nextX += SnakeSquareSize;
                if (nextX >= GameArea.ActualWidth){
                    nextX = 0;
                    nextY += SnakeSquareSize;
                    rowCounter++;
                    nextIsOdd = (rowCounter % 2 != 0);
                }
                if (nextY >= GameArea.ActualHeight)
                    doneDrawingBackground = true;
            }
        }
        private void DrawSnake(){
            foreach (SnakePart snakePart in snakeParts){
                if (snakePart.UiElement == null){
                    snakePart.UiElement = new Rectangle(){
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush)
                    };
                    GameArea.Children.Add(snakePart.UiElement);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                }
            }
        }
        private void DrawMines()
        {
            foreach (Mine mine in mines){
                if (mine.UiElement == null){
                    mine.UiElement = new Ellipse(){
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = mineBrush
                    };
                    GameArea.Children.Add(mine.UiElement);
                    Canvas.SetTop(mine.UiElement, mine.Position.Y);
                    Canvas.SetLeft(mine.UiElement, mine.Position.X);
                }
            }
        }
        private Point GetNextMinePosition()
        {
            int maxX = (int)(GameArea.ActualWidth / SnakeSquareSize);
            int maxY = (int)(GameArea.ActualHeight / SnakeSquareSize);
            int mineX = rnd.Next(0, maxX) * SnakeSquareSize;
            int mineY = rnd.Next(0, maxY) * SnakeSquareSize;
            foreach (SnakePart snakePart in snakeParts){
                if ((snakePart.Position.X == mineX) && (snakePart.Position.Y == mineY))
                    return GetNextMinePosition();
            }
            return new Point(mineX, mineY);
        }
        private void MoveSnake(){
            while (snakeParts.Count >= snakeLength){
                GameArea.Children.Remove(snakeParts[0].UiElement);
                snakeParts.RemoveAt(0);
            } 
            foreach (SnakePart snakePart in snakeParts){
                (snakePart.UiElement as Rectangle).Fill = snakeBodyBrush;
                snakePart.IsHead = false;
            }
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (snakeDirection){
                case SnakeDirection.Left:
                    if (snakeHead.Position.X < 20){
                        nextX = GameArea.ActualWidth-20;
                        break;
                    }
                    nextX -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    if (snakeHead.Position.X >= GameArea.ActualWidth-20){
                        nextX = 0;
                        break;
                    }
                    nextX += SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    if (snakeHead.Position.Y < 20){
                        nextY = GameArea.ActualHeight-20;
                        break;
                    }
                    nextY -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    if (snakeHead.Position.Y >= GameArea.ActualHeight-20){
                        nextY = 0;
                        break;
                    }
                    nextY += SnakeSquareSize;
                    break;
            } 
            snakeParts.Add(new SnakePart(){
                Position = new Point(nextX, nextY),
                IsHead = true
            });
            DrawSnake();
            DoCollisionCheck();          
        }
        private void DoCollisionCheck()
        {
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];
            if ((snakeHead.Position.X == Canvas.GetLeft(snakeFood)) && (snakeHead.Position.Y == Canvas.GetTop(snakeFood))){
                EatSnakeFood();
                return;
            }
            foreach (Mine mine in mines){
                if ((snakeHead.Position.X == mine.Position.X) && (snakeHead.Position.Y == mine.Position.Y)){
                    EndGame();
                    return;
                }
            }
            /*
            if ((snakeHead.Position.Y < 0) || (snakeHead.Position.Y >= GameArea.ActualHeight) ||
            (snakeHead.Position.X < 0) || (snakeHead.Position.X >= GameArea.ActualWidth)){
                EndGame();
            }*/

            foreach (SnakePart snakeBodyPart in snakeParts.Take(snakeParts.Count - 1)){
                if ((snakeHead.Position.X == snakeBodyPart.Position.X) && (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                    EndGame();
            }
        }

        private void GameTickTimer_Tick(object sender, EventArgs e){
            MoveSnake();
        }
        private void StartNewGame(){
            bdrPauseMessage.Visibility = Visibility.Collapsed;
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Collapsed;
            bdrEndOfGame.Visibility = Visibility.Collapsed;
            foreach (SnakePart snakeBodyPart in snakeParts){
                if (snakeBodyPart.UiElement != null)
                    GameArea.Children.Remove(snakeBodyPart.UiElement);
            }
            foreach (Mine mine in mines){
                if (mine.UiElement != null)
                    GameArea.Children.Remove(mine.UiElement);
            }
            mines.Clear();
            snakeParts.Clear();
            if (snakeFood != null)
                GameArea.Children.Remove(snakeFood);
            currentScore = 0;
            snakeLength = SnakeStartLength;
            snakeDirection = SnakeDirection.Right;
            snakeParts.Add(new SnakePart() { Position = new Point(SnakeSquareSize * 5, SnakeSquareSize * 5) });
            mines.Add(new Mine() { Position = GetNextMinePosition()});
            mines.Add(new Mine() { Position = GetNextMinePosition()});
            mines.Add(new Mine() { Position = GetNextMinePosition()});
            mines.Add(new Mine() { Position = GetNextMinePosition()});
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);
            DrawSnake();
            DrawMines();
            DrawSnakeFood();
            UpdateGameStatus();       
            gameTickTimer.IsEnabled = true;
        }
        private void GamePause(){
            bdrPauseMessage.Visibility = Visibility.Visible;
            gameTickTimer.IsEnabled = false;
        }
        private void GameContinue(){
            bdrPauseMessage.Visibility = Visibility.Collapsed;
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Collapsed;
            bdrEndOfGame.Visibility = Visibility.Collapsed;
            gameTickTimer.IsEnabled = true;
        }
        private Point GetNextFoodPosition(){
            int maxX = (int)(GameArea.ActualWidth / SnakeSquareSize);
            int maxY = (int)(GameArea.ActualHeight / SnakeSquareSize);
            int foodX = rnd.Next(0, maxX) * SnakeSquareSize;
            int foodY = rnd.Next(0, maxY) * SnakeSquareSize;

            foreach (SnakePart snakePart in snakeParts){
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                    return GetNextFoodPosition();
            }
            foreach (Mine mine in mines){
                if  ((mine.Position.Y == foodY) && (mine.Position.X == foodX))
                    return GetNextFoodPosition();
            }
            return new Point(foodX, foodY);
        }
        private void DrawSnakeFood(){
            Point foodPosition = GetNextFoodPosition();
            snakeFood = new Ellipse(){
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Fill = foodBrush
            };
            GameArea.Children.Add(snakeFood);
            Canvas.SetTop(snakeFood, foodPosition.Y);
            Canvas.SetLeft(snakeFood, foodPosition.X);
        }
        private void Window_KeyUp(object sender, KeyEventArgs e){
            SnakeDirection originalSnakeDirection = snakeDirection;
            switch (e.Key){
                case Key.Up:
                    if (gameTickTimer.IsEnabled == false) { break; }
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;
                case Key.Down:
                    if (gameTickTimer.IsEnabled == false) { break; }
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;
                case Key.Left:
                    if (gameTickTimer.IsEnabled == false) { break; }
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;
                case Key.Right:
                    if (gameTickTimer.IsEnabled == false) { break; }
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;
                case Key.Space:
                    StartNewGame();
                    break;
                case Key.Escape:
                    if((bdrHighscoreList.Visibility == Visibility.Visible)||(bdrEndOfGame.Visibility == Visibility.Visible)||(bdrNewHighscore.Visibility == Visibility.Visible) ||(bdrWelcomeMessage.Visibility == Visibility.Visible)) {
                        break; }
                    if (gameTickTimer.IsEnabled == false) {
                        GameContinue();
                        break; }
                    if (gameTickTimer.IsEnabled == true)
                        GamePause();
                    break;
            }
            if (snakeDirection != originalSnakeDirection)
                MoveSnake();
        }
       
        private void EatSnakeFood(){
            snakeLength++;
            currentScore++;
            int timerInterval = Math.Max(SnakeSpeedThreshold, (int)gameTickTimer.Interval.TotalMilliseconds - (currentScore * 2));
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            GameArea.Children.Remove(snakeFood);
            if (currentScore % 2 == 0) {
                mines.Add(new Mine() { Position = GetNextMinePosition() });
                DrawMines();
            }
            DrawSnakeFood();
            UpdateGameStatus();
        }
        private void UpdateGameStatus(){
            this.tbStatusScore.Text = currentScore.ToString();
            this.tbStatusSpeed.Text = gameTickTimer.Interval.TotalMilliseconds.ToString();
        }
        private void EndGame(){
            bool isNewHighscore = false;
            if (currentScore > 0){
                int lowestHighscore = (this.HighscoreList.Count > 0 ? this.HighscoreList.Min(x => x.Score) : 0);
                if ((currentScore > lowestHighscore) || (this.HighscoreList.Count < MaxHighscoreListEntryCount)){
                    bdrNewHighscore.Visibility = Visibility.Visible;
                    txtPlayerName.Focus();
                    isNewHighscore = true;
                }
            }
            if (!isNewHighscore){
                tbFinalScore.Text = currentScore.ToString();
                bdrEndOfGame.Visibility = Visibility.Visible;
            }
            gameTickTimer.IsEnabled = false;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e){
            try { this.DragMove(); }
            catch (System.InvalidOperationException) {}
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e){
            this.Close();
        }
        private void BtnShowHighscoreList_Click(object sender, RoutedEventArgs e){
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;
        }
        private void LoadHighscoreList(){
            if (File.Exists("snake_highscorelist.xml")){
                XmlSerializer serializer = new XmlSerializer(typeof(List<SnakeHighscore>));
                using (Stream reader = new FileStream("snake_highscorelist.xml", FileMode.Open)){
                    List<SnakeHighscore> tempList = (List<SnakeHighscore>)serializer.Deserialize(reader);
                    this.HighscoreList.Clear();
                    foreach (var item in tempList.OrderByDescending(x => x.Score))
                        this.HighscoreList.Add(item);
                }
            }
        }
        private void SaveHighscoreList(){
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<SnakeHighscore>));
            using (Stream writer = new FileStream("snake_highscorelist.xml", FileMode.Create)){
                serializer.Serialize(writer, this.HighscoreList);
            }
        }
        private void BtnAddToHighscoreList_Click(object sender, RoutedEventArgs e){
            int newIndex = 0;
            txtPlayerName.Text = wpfsnake.MainWindow.Playername;
            if ((this.HighscoreList.Count > 0) && (currentScore < this.HighscoreList.Max(x => x.Score))){
                SnakeHighscore justAbove = this.HighscoreList.OrderByDescending(x => x.Score).First(x => x.Score >= currentScore);
                if (justAbove != null)
                    newIndex = this.HighscoreList.IndexOf(justAbove) + 1;
            }
            this.HighscoreList.Insert(newIndex, new SnakeHighscore(){
                PlayerName = wpfsnake.MainWindow.Playername,
                Score = currentScore
            });
            while (this.HighscoreList.Count > MaxHighscoreListEntryCount)
                this.HighscoreList.RemoveAt(MaxHighscoreListEntryCount);
            SaveHighscoreList();
            bdrNewHighscore.Visibility = Visibility.Collapsed;
            bdrHighscoreList.Visibility = Visibility.Visible;
        }
        private void btnBack_Click(object sender, RoutedEventArgs e){
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
