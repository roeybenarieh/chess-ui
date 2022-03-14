using chessair_client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace connect4_client
{
    public class promotion_hundler
    {
        private readonly chess chess;
        private Button[] peace;
        private int i_cor;
        private static readonly int[] promotion_peaces = {4,3,2,1 };
        //works if black or white
        public promotion_hundler(int i_cor, chess chess,bool show)
        {
            int correction = 0;
            if (chess.iswhite)
                correction = 6;
            this.i_cor = i_cor;
            this.chess = chess;
            this.peace = new Button[4];
            for (int i = 0; i < this.peace.Length; i++)
            {
                peace[i] = new Button();
                peace[i].Name = promotion_peaces[i].ToString();
                peace[i].Location = new Point(chess.boarders_from_window_verticale + (chess.rectangesize * i_cor), chess.boarders_from_window_diagonal + (chess.rectangesize * i));
                peace[i].Size = new Size(chess.rectangesize, chess.rectangesize);
                peace[i].FlatStyle = FlatStyle.Flat;
                peace[i].FlatAppearance.BorderSize = 5;
                peace[i].BackColor = Color.Honeydew;
                peace[i].FlatAppearance.BorderColor = peace[i].BackColor;
                peace[i].BackgroundImageLayout = ImageLayout.Center;
                peace[i].Click += new System.EventHandler(this.pick_promotion);
                peace[i].Visible = true;
                chess.add_peace(peace[i], promotion_peaces[i]+correction);
            }
            if (!show)
                stop_show();
            for (int i = 0; i < this.peace.Length; i++)
            {
                chess.Controls.Add(peace[i]);
                peace[i].BringToFront();
            }
        }

        private void pick_promotion(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int row = chess.iswhite ? 0 : 7;

            chess.send_move_to_server(chess.chartointposition(chess.xymarkedpeace[2]) ,
                chess.chartointposition(chess.xymarkedpeace[0]) ,
                row,
                7 - this.i_cor,
                button.Name);
            stop_show();
        }
        public void stop_show()
        {
            for (int i = 0; i < this.peace.Length; i++)
                peace[i].Visible = false;
        }

    }
}
