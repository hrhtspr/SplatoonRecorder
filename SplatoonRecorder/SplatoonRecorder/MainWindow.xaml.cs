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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace SplatoonRecorder
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public static List<string> Weapons;
        public static List<string> Stages;
        public static List<string> Udemaes;
        const string ConfigFileName = "config.csv";
        public MainWindow()
        {
            InitializeComponent();
            ConfigLoad();
            Initialize();
            Clear();
        }
        public void Initialize()
        {
            battleType.ItemsSource = Enum.GetValues(typeof(BattleType));
            weapon.SelectedIndex = 0;
            stage1.SelectedIndex = 0;
            stage2.SelectedIndex = 1;
            stage3.SelectedIndex = 2;
            stg1.IsChecked = true;
            battleType.SelectedIndex = 0;
            resultWin.IsChecked = true;
            udemae.SelectedIndex = 0;
        }

        public void Clear()
        {
            this.nawabariNuri.Text = "";
            this.kill.Text = this.death.Text = "";
            this.resultKO.IsChecked = this.resultBehind.IsChecked = false;
            this.kaisenMikata.IsChecked = this.kaisenTeki.IsChecked = false;
        }

        public void ConfigLoad()
        {
            if (File.Exists(ConfigFileName))
            {
                using (var sr = new StreamReader(ConfigFileName))
                {
                    var str = sr.ReadLine();
                    Weapons = str.Split(',').ToList();
                    str = sr.ReadLine();
                    Stages = str.Split(',').ToList();
                    str = sr.ReadLine();
                    Udemaes = str.Split(',').ToList();
                }
                weapon.ItemsSource = Weapons;
                stage1.ItemsSource = stage2.ItemsSource = stage3.ItemsSource = Stages;
                udemae.ItemsSource = Udemaes;
            }
            else
            {
                this.Close();
            }
        }
        public bool DataCheck()
        {
            var battleType = (BattleType)this.battleType.SelectedValue;
            if (battleType == BattleType.ナワバリ)
            {
                if (this.nawabariNuri.Value.HasValue)
                {
                    return true;
                }
                else
                {
                    message.Content = "塗りポイントを記入してください";
                }
            }
            else if (battleType == BattleType.フェス)
            {
                return true;
            }
            else
            {
                if (this.udemaePoint.Value.HasValue)
                {
                    return true;
                }
                else
                {
                    message.Content = "ウデマエポイントを記入してください";
                }
            }
            return false;
        }
        public void DataAdd()
        {
            var data = new BattleData();
            try
            {
                data.Weapon = this.weapon.SelectedValue.ToString();
                data.Stage = (this.stg1.IsChecked.Value ? this.stage1.SelectedValue : this.stg2.IsChecked.Value ? this.stage2.SelectedValue : this.stage3.SelectedValue).ToString();
                data.BattleType = (BattleType)this.battleType.SelectedIndex;
                data.IsTag = data.BattleType != BattleType.ナワバリ && this.isTag.IsChecked.Value;
                data.IsWin = this.resultWin.IsChecked.Value;
                data.Result = this.resultKO.IsChecked.Value ? Result.ノックアウト : this.resultBehind.IsChecked.Value ? Result.延長逆転 : Result.タイムアップ;
                data.KaisenMikata = this.kaisenMikata.IsChecked.Value;
                data.KaisenTeki = this.kaisenTeki.IsChecked.Value;
                data.Nuri = (short)(data.BattleType == BattleType.ナワバリ ? this.nawabariNuri.Value.Value : -1);
                data.Kill = (sbyte)(this.kill.Value.HasValue ? this.kill.Value.Value : -1);
                data.Death = (sbyte)(this.death.Value.HasValue ? this.death.Value.Value : -1);
                data.Udemae = data.BattleType != BattleType.ナワバリ && data.BattleType != BattleType.フェス ? this.udemae.SelectedValue.ToString() : "";
                data.UdemaePoint = (sbyte)(data.BattleType != BattleType.ナワバリ && data.BattleType != BattleType.フェス ? this.udemaePoint.Value.Value : -1);
            }
            catch (Exception)
            {
                return;
            }
            DataWrite(data);
        }
        public void DataWrite(BattleData data)
        {
            var dataFileName = this.name.Text + ".csv";
            if (!File.Exists(dataFileName))
            {
                using (var sw = new StreamWriter(dataFileName, true,Encoding.UTF8))
                {
                    sw.WriteLine(BattleData.DataLabel);
                }
            }
            using (var sw = new StreamWriter(dataFileName, true, Encoding.UTF8))
            {
                sw.WriteLine(data.ToString());
            }
        }

        private void battleType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var battleType = (BattleType)this.battleType.SelectedValue;
            if (battleType == BattleType.ナワバリ || battleType == BattleType.フェス)
            {

                this.gachiOnly.Visibility = System.Windows.Visibility.Hidden;
                this.gachiEx.IsEnabled = false;
                this.isTag.IsEnabled = false;
                this.isTag.IsChecked = false;
                if (battleType == BattleType.ナワバリ)
                {
                    this.fes.Visibility = System.Windows.Visibility.Hidden;
                    this.nawabariOnly.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.fes.Visibility = System.Windows.Visibility.Visible;
                    this.nawabariOnly.Visibility = System.Windows.Visibility.Hidden;
                    if (this.battleType.SelectedIndex >= 2)
                    {
                        this.battleType.SelectedIndex = 0;
                    }
                }
                message.Content = "";
            }
            else
            {
                this.nawabariOnly.Visibility = System.Windows.Visibility.Hidden;
                this.gachiOnly.Visibility = System.Windows.Visibility.Visible;
                this.gachiEx.IsEnabled = true;
                this.isTag.IsEnabled = true;
                this.fes.Visibility = System.Windows.Visibility.Hidden;
            }
        }


        private void resultKO_Checked(object sender, RoutedEventArgs e)
        {
            this.resultBehind.IsChecked = false;
        }

        private void resultBehind_Checked(object sender, RoutedEventArgs e)
        {
            this.resultKO.IsChecked = false;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            if (DataCheck())
            {
                DataAdd();
                Clear();
                message.Content = "データを追加しました";
                await Task.Delay(1500);
                message.Content = "";
            }
        }

        private void Analysis_Click(object sender, RoutedEventArgs e)
        {
            var window = new AnalysisView(name.Text);
            window.Show();
        }


    }
}
