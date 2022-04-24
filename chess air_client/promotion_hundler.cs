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
    public class Promotion_hundler
    {
        private readonly Chess chess;
        private readonly Button[] peace;
        private readonly int i_cor;
        private static readonly int[] promotion_peaces = {4,3,2,1 };
        //works if black or white
        public Promotion_hundler(int i_cor, Chess chess,bool show)
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
                peace[i].Location = new Point(Chess.boarders_from_window_verticale + (Chess.squeresize * i_cor), Chess.boarders_from_window_diagonal + (Chess.squeresize * i));
                peace[i].Size = new Size(Chess.squeresize, Chess.squeresize);
                peace[i].FlatStyle = FlatStyle.Flat;
                peace[i].FlatAppearance.BorderSize = 5;
                peace[i].BackColor = Color.Honeydew;
                peace[i].FlatAppearance.BorderColor = peace[i].BackColor;
                peace[i].BackgroundImageLayout = ImageLayout.Center;
                peace[i].Click += new System.EventHandler(this.Pick_promotion);
                peace[i].Visible = true;
                chess.Add_peace(peace[i], promotion_peaces[i]+correction);
            }
            if (!show)
                Stop_show();
            for (int i = 0; i < this.peace.Length; i++)
            {
                chess.Controls.Add(peace[i]);
                peace[i].BringToFront();
            }
        }

        private void Pick_promotion(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int row = chess.iswhite ? 0 : 7;

            chess.Send_move_to_server(chess.Chartointposition(chess.xymarkedpeace[0]) ,
                chess.Chartointposition(chess.xymarkedpeace[2]) ,
                row,
                Math.Abs(row - this.i_cor),
                button.Name);
            Stop_show();
        }
        public void Stop_show()
        {
            for (int i = 0; i < this.peace.Length; i++)
                peace[i].Visible = false;
        }

    }
}
