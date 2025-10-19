using ZedGraph;

namespace PSO_Algoritması_YapayZekaOdev6
{
    public partial class Form1 : Form
    {
        public class ParcacikVerisi
        {
            public double x1 { get; set; }
            public double x2 { get; set; }
            public double fitness { get; set; }
            public double pbest { get; set; }
            public double v1 { get; set; }
            public double v2 { get; set; }
            public double rand1 { get; set; }
            public double rand2 { get; set; }
        }

        List<ParcacikVerisi> parcacikListesi = new List<ParcacikVerisi>();
        List<double> gbestDegerleri = new List<double>();

        double N;
        double G;
        double C1;
        double C2;

        int sonKriter;

        int round = 0;

        #region Sonlandırma Kriterleri
        // Jenerasyon Sayısı
        double eski_gbest = double.MaxValue;
        double yeni_gbest;
        double degismeyen_adim = 0;

        // Yakınsama Değeri

        #endregion

        ZedGraphControl zedGraphControl1;

        public Form1()
        {
            InitializeComponent();
            GrafikCiz();
        }

        // Başlangıç
        private void button1_Click(object sender, EventArgs e)
        {
            BaslangicParametreleri();

            for (int i = 0; i < G; i++)       // Jenerasyon değeri kadar döngü çalışır (G)
            {

                for (int j = 0; j < N; j++)     //1.Adım
                {
                    RastgeleParcacik(j);        // Rastgele parçacıklar üretilerek fitness değeri bulunur (ilk iterasyon -> pbest = fitness)
                }

                for (int j = 0; j < N; j++)     //2.Adım
                {
                    parcacik_hizi_hesap(j);     // Parcaciklarin hızı ölçülür
                }

                for (int j = 0; j < N; j++)     //3.Adım
                {
                    yeni_xDeger_hesap(i);       // Burada x1 ve x2 değerleri -> x1 = (x1 + v1) ve x2 = (x2 + v2) ile güncellenir ve döngü tekrardan başlar
                }

                
                if (sonlandirma_kriteri_hesap(sonKriter)) // Sonlandırma Kriteri
                {
                    break;
                } 

                round++;
            }
            
            GbestGrafikCiz();

            label5.Text = $"Min Obj = {enIyi_gbest()}";
        }

        // Başlangıç
        public void BaslangicParametreleri()
        {
            N = (double)numericUpDown1.Value;           // Parçacık sayısı (N)   ->  N = 10 varsayılmıştır
            G = (double)numericUpDown2.Value;           // Jenerasyon Sayısı (G)
            C1 = (double)numericUpDown3.Value;          // gbest öğr. fakt. (C1) -> c1 = 2 varsayılmıştır
            C2 = (double)numericUpDown4.Value;          // pbest öğr. fakt. (C2) -> c2 = 2 varsayılmıştır

            if (radioButton1.Checked)
            {
                sonKriter = 0;
            }
            else if (radioButton2.Checked)
            {
                sonKriter = 1;
            } 
            else if (radioButton3.Checked)
            {
                sonKriter = 2;
            }
        }

        #region 1.Adım
        // 1
        public void RastgeleParcacik(int index)
        {
            Random rnd = new Random();
            double x1 = rnd.NextDouble()*10-5;    // (-5 <= x1,x2 <= 5)
            double x2 = rnd.NextDouble()*10-5;

            double fitness = camelBack_hesap(x1, x2);

            if (round == 0)
            {
                ParcacikEkle(x1, x2, fitness, fitness); // İLK İTERASYONDA PBEST KENDİ FİTNESS DEĞERİNİ ALIR
            }
            else
            {
                parcacikListesi[index].x1 = x1;
                parcacikListesi[index].x2 = x2;

                if (fitness < parcacikListesi[index].pbest)
                {
                    parcacikListesi[index].fitness = fitness;
                    parcacikListesi[index].pbest = fitness;
                }
                else
                {
                    parcacikListesi[index].fitness = fitness;
                }
            }
            
        }

        // 1.1
        public double camelBack_hesap(double x1, double x2)
        {   
            double fx = 4*Math.Pow(x1,2) - 2.1*Math.Pow(x1,4) + (1/3)*Math.Pow(x1,6) + x1*x2 - 4*Math.Pow(x2,2) + 4*Math.Pow(x2,4);

            return fx;
        }

        // 1.2
        public void ParcacikEkle(double x1, double x2, double fitness, double pbest)
        {
            ParcacikVerisi veri = new ParcacikVerisi()
            {
                x1 = x1,
                x2 = x2,
                fitness = fitness,
                pbest = pbest,
                v1 = 0,
                v2 = 0,
                rand1 = 0,
                rand2 = 0,
            };

            parcacikListesi.Add(veri);
        }
        #endregion

        #region 2.ADIM
        // 2
        public void parcacik_hizi_hesap(int index)
        {
            Random rnd = new Random();

            double rand1 = rnd.NextDouble();
            double rand2 = rnd.NextDouble();

            double pbest = parcacikListesi[index].pbest;
            double gbest = enIyi_gbest();

            double v01 = v_degeri_al(index, 0);    //Bir önceki satırın v değerleri
            double v02 = v_degeri_al(index, 1);

            double c1 = C1;
            double c2 = C2;

            double x1 = parcacikListesi[index].x1;
            double x2 = parcacikListesi[index].x2;

            double v1 = ParcacikFormulu(pbest, gbest, v01, c1, c2, x1, rand1, rand2); // PARCACIK HIZI (V1)
            double v2 = ParcacikFormulu(pbest, gbest, v02, c1, c2, x2, rand1, rand2); // PARCACIK HIZI (V2)

            parcacikListesi[index].v1 = v1;
            parcacikListesi[index].v2 = v2;
            parcacikListesi[index].rand1 = rand1;
            parcacikListesi[index].rand2 = rand2;
        }

        // 2.1
        public double enIyi_gbest()
        {
            double min = double.MaxValue;
            for (int i = 0; i < parcacikListesi.Count; i++)
            {
                if (parcacikListesi[i].pbest <= min)
                {
                    min = parcacikListesi[i].pbest;
                }
            }
            return min;
        }

        // 2.2
        public double v_degeri_al(int index, int v)
        {
            if (index == 0)
            {
                return 0;
            }
            else if (v == 0)
            {
                double vk1 = parcacikListesi[index].v1;
                return vk1;
            }
            else if (v == 1)
            {
                double vk2 = parcacikListesi[index].v2;
                return vk2;
            }
            return 0;
        }

        // 2.3
        public double ParcacikFormulu(double pbest, double gbest, double v0, double c1, double c2, double x, double rand1, double rand2)
        {
            double formul = (v0 + c1 * rand1 * (pbest - x) + c2 * rand2 * (gbest - x));

            return formul;
        }
        #endregion

        #region 3.Adım
        // 3
        public void yeni_xDeger_hesap(int index)
        {
            double x1 = parcacikListesi[index].x1;
            double x2 = parcacikListesi[index].x2;
            
            double v1 = parcacikListesi[index].v1;
            double v2 = parcacikListesi[index].v2;

            double x11 = x1 + v1;
            double x12 = x2 + v2;

            parcacikListesi[index].x1 = x11;     //BUNLAR YENİ x1 ve x2 değerleridir
            parcacikListesi[index].x2 = x12;     //BURADAN İTABAREN DÖNGÜ YENİDEN BAŞLAR JENERASYON SAYISI KADAR TEKRARLANIR
        }
        #endregion

        #region Sonlandırma Kriteri
        public bool sonlandirma_kriteri_hesap(int stopCmd)
        {
            double yeni_gbest = enIyi_gbest();
            gbestDegerleri.Add(yeni_gbest);

            if (stopCmd == 0) // 0: Jenerasyon sınırına göre sonlandırma
            {
                if (round == G)
                {
                    MessageBox.Show("Maksimum jenerasyon sayısına ulaşıldı.");
                    return true;
                }
            }

            else if(stopCmd == 1)
            {
                if (Math.Abs(yeni_gbest - eski_gbest) < 1e-6)
                {
                    degismeyen_adim++;
                    if (degismeyen_adim >= G)
                    {
                        MessageBox.Show("gbest değeri sürekli olarak aynı kaldığı için durduruldu");
                        return true;
                    }
                }
                else
                {
                    degismeyen_adim = 0;
                    eski_gbest = yeni_gbest;
                    return false;
                }
                return false;
            }

            else if(stopCmd == 2)
            {
                if(yeni_gbest <= -1200)
                {
                    MessageBox.Show($"Yakınsama grafiğinde -1200 değerini aşan bir değer bulundu.\n Jenerasyon durduruluyor...");
                    return true;
                }
            }
            return false;
        }

        #endregion

        private void GbestGrafikCiz()
        {
            GraphPane pane = zedGraphControl1.GraphPane;
            pane.CurveList.Clear();
            pane.Title.Text = "Gbest Yakınsama Grafiği";
            pane.XAxis.Title.Text = "İterasyon";
            pane.YAxis.Title.Text = "Gbest Değeri";

            PointPairList gbestListesi = new PointPairList();

            for (int i = 0; i < gbestDegerleri.Count; i++)
                gbestListesi.Add(i + 1, gbestDegerleri[i]);

            LineItem gbestEgrisi = pane.AddCurve("Gbest", gbestListesi, Color.Blue, SymbolType.Circle);
            gbestEgrisi.Line.Width = 2.0F;

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }

        public void GrafikCiz()
        {
            zedGraphControl1 = new ZedGraphControl();
            zedGraphControl1.Name = "zedGraphControl1";
            zedGraphControl1.Location = new Point(320, 10); // konum
            zedGraphControl1.Size = new Size(750, 500);    // boyut
            zedGraphControl1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            this.Controls.Add(zedGraphControl1);
        }
    }
}