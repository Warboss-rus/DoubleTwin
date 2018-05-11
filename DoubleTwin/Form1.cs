using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using System.Net.Sockets;

namespace DoubleTwin
{
	public partial class Form1 : Form
	{
		private Bitmap background;
		private Bitmap cardBackgroundBack;
		private Bitmap cardBackgroundPlayer;
		private Bitmap cardBackgroundEnemy;
		private List<sCard> cards;
		private PlayerInterface player1;
		private PlayerInterface player2;
		private PlayerInterface currentPlayer;
		private int selectedCardIndex;
		private Table table;
		private bool dragNDrop;
		private int animationPercent;
		private TcpClient connection;

		public Form1()
		{
			InitializeComponent();

			LoadCards();
			PrepareBackgrounds();

			ResizeRedraw = true;
			backgroundWorker1.RunWorkerAsync();
		}

		private void PrepareBackgrounds()
		{
			Image bg = Image.FromFile(@"img/center_bg.jpg");
			Image sideBg = Image.FromFile(@"img/side_bg.jpg");
			//prepare background
			background = new Bitmap(1868, 1024);
			Graphics canvas = Graphics.FromImage(background);
			Rectangle sideBackgroundRect = new Rectangle(0, 0, 512, 1024);
			canvas.DrawImage(sideBg, sideBackgroundRect);
			sideBackgroundRect.X += 1356;
			canvas.DrawImage(sideBg, sideBackgroundRect);
			Rectangle backgroundRect = new Rectangle(512, 0, 844, 1024);
			canvas.DrawImage(bg, backgroundRect);
			canvas.Save();
			bg.Dispose();
			sideBg.Dispose();
		}

		private static Random rnd = new Random();

		private List<sCard> GenerateCards()
		{

			List<sCard> result = new List<sCard>();
			for (int i = 0; i < 5; ++i)
			{
				result.Add(cards[rnd.Next(0, cards.Count)]);
			}
			return result;
		}

		private delegate void FuncDelegate();
		private delegate bool BoolDelegate();


		private void timer1_Tick(object sender, EventArgs e)
		{
			animationPercent += (timer1.Interval / 10);
			if (animationPercent >= 100)
			{
				timer1.Stop();
				table.EndFlippingAnimation();
				Invoke(new BoolDelegate(CheckForGameOver));
			}
			if (animationPercent >= 50 && (animationPercent - timer1.Interval / 10) < 50)
			{
				table.FlipCardsMarkedToBeFlipped();
			}
			Refresh();
		}

		private Bitmap GenerateCardBitmap(Image characters, Image numbers, Image frame, int column, int row, sCard card)
		{
			Rectangle dest = new Rectangle(0, 0, 256, 316);
			//Prepare image
			Bitmap image = new Bitmap(256, 316);
			Graphics canvas = Graphics.FromImage(image);
			Rectangle imageRect = new Rectangle(column * 256, row * 316, 256, 316);
			//Draw character
			canvas.DrawImage(characters, dest, imageRect, GraphicsUnit.Pixel);
			//Draw numbers frame
			Rectangle rect = new Rectangle(256, 0, 256, 316);
			canvas.DrawImage(frame, dest, rect, GraphicsUnit.Pixel);
			//Draw numbers
			Rectangle numberRect = new Rectangle(dest.Left + 10, dest.Top + (dest.Height - 32) / 2, 32, 32);
			DrawNumber(canvas, numberRect, card.left, numbers);
			numberRect = new Rectangle(dest.Right - 10 - 32, dest.Top + (dest.Height - 32) / 2, 32, 32);
			DrawNumber(canvas, numberRect, card.right, numbers);
			numberRect = new Rectangle(dest.Left + (dest.Width - 32) / 2, dest.Top + 10, 32, 32);
			DrawNumber(canvas, numberRect, card.top, numbers);
			numberRect = new Rectangle(dest.Left + (dest.Width - 32) / 2, dest.Bottom - 10 - 32, 32, 32);
			DrawNumber(canvas, numberRect, card.bottom, numbers);
			canvas.Save();
			return image;
		}

		private void GenerateCardBackgrounds(Image frame)
		{
			Rectangle dest = new Rectangle(0, 0, 256, 316);
			cardBackgroundBack = new Bitmap(256, 316);
			using (Graphics g = Graphics.FromImage(cardBackgroundBack))
			{
				Rectangle rect = new Rectangle(0, 0, 256, 316);
				g.DrawImage(frame, dest, rect, GraphicsUnit.Pixel);
			}
			cardBackgroundPlayer = new Bitmap(256, 316);
			using (Graphics g = Graphics.FromImage(cardBackgroundPlayer))
			{
				Rectangle rect = new Rectangle(512, 0, 256, 316);
				g.DrawImage(frame, dest, rect, GraphicsUnit.Pixel);
			}
			cardBackgroundEnemy = new Bitmap(256, 316);
			using (Graphics g = Graphics.FromImage(cardBackgroundEnemy))
			{
				Rectangle rect = new Rectangle(768, 0, 256, 316);
				g.DrawImage(frame, dest, rect, GraphicsUnit.Pixel);
			}
		}

		private void LoadCards()
		{
			cards = new List<sCard>();

			Image characters = Image.FromFile(@"img/card_types.png");
			Image numbers = Image.FromFile(@"img/power_numbers.png");
			Image frame = Image.FromFile(@"img/card_frame.png");

			XmlDocument doc = new XmlDocument();
			doc.Load("cards.xml");

			XmlNodeList cardsNodes = doc.DocumentElement.SelectNodes("/cards/card");
			foreach (XmlNode cardNode in cardsNodes)
			{
				sCard card = new sCard();
				card.name = cardNode.Attributes["name"].Value;
				card.left = Convert.ToInt32(cardNode.Attributes["left"].Value);
				card.right = Convert.ToInt32(cardNode.Attributes["right"].Value);
				card.top = Convert.ToInt32(cardNode.Attributes["top"].Value);
				card.bottom = Convert.ToInt32(cardNode.Attributes["bottom"].Value);
				int column = Convert.ToInt32(cardNode.Attributes["column"].Value) - 1;
				int row = Convert.ToInt32(cardNode.Attributes["row"].Value) - 1;
				card.image = GenerateCardBitmap(characters, numbers, frame, column, row, card);
				cards.Add(card);
			}
			characters.Dispose();
			numbers.Dispose();

			GenerateCardBackgrounds(frame);
			frame.Dispose();
		}

		private void DrawNumber(Graphics canvas, Rectangle dest, int number, Image numbers)
		{
			number = number % 10;
			Rectangle rect = new Rectangle(number * 32, 0, 32, 32);
			canvas.DrawImage(numbers, dest, rect, GraphicsUnit.Pixel);
		}

		private void NewGame()
		{
			timer1.Interval = 1000 / Convert.ToInt32(numericUpDown1.Value);
			connection = null;
			table = new Table();
			selectedCardIndex = -1;
			if (comboBox2.SelectedItem.ToString() == "Network Player")
			{
				if (radioButton1.Checked)// We are host
				{
					TcpListener serverSocket = new TcpListener(Convert.ToInt32(textBox2.Text));
					serverSocket.Start();
					connection = serverSocket.AcceptTcpClient();
					serverSocket.Stop();
				}
				else
				{
					connection = new TcpClient(textBox1.Text, Convert.ToInt32(textBox2.Text));
				}
				connection.ReceiveTimeout = Int32.MaxValue;
			}
			player1 = GeneratePlayer(comboBox1.SelectedItem.ToString(), false);
			player2 = GeneratePlayer(comboBox2.SelectedItem.ToString(), true);
			currentPlayer = player1;
			if (radioButton2.Checked && connection != null)
			{
				currentPlayer = player2;
			}
		}

		private PlayerInterface GeneratePlayer(String type, bool enemy)
		{
			if (type == "Local Player")
			{
				return new LocalPlayer(GenerateCards());
			}
			else if (type == "SimpleAI")
			{
				return new SimpleAI(GenerateCards(), enemy);
			}
			else if (type == "BaseAI")
			{
				return new BaseAI(GenerateCards(), enemy);
			}
			else if (type == "EnchAI")
			{
				return new EnchAI(GenerateCards(), enemy);
			}
			else if (type == "Network Player")
			{
				return new NetworkPlayer(cards, connection);
			}
			else
			{
				throw new Exception("Not yet implemented");
			}
		}

		public delegate void AddCardDelegate(sTableCard tcard);

		private void AddCard(sTableCard tcard)
		{
			table.AddCard(tcard);
			animationPercent = 0;//start animation
			timer1.Start();
			ChangePlayer();
		}

		private void ChangePlayer()
		{
			if (currentPlayer.Equals(player1))
				currentPlayer = player2;
			else
				currentPlayer = player1;
			Refresh();
		}

		public bool CheckForGameOver()
		{
			if (currentPlayer == null) return false;
			if (table.GetNumberOfCards() == 9)
			{
				currentPlayer = null;
				if (connection != null) connection.Close();
				timer1.Stop();
				int playerCardsCount = table.GetNumberOfCardsPlayerOwns(false);
				int enemyCardsCount = table.GetNumberOfCardsPlayerOwns(true);
				if (playerCardsCount > enemyCardsCount)
				{
					MessageBox.Show("You win!");
				}
				else
				{
					MessageBox.Show("You lose!");
				}
				panel1.Visible = true;
				Refresh();
				return true;
			}
			return false;
		}

		private void Form1_Click(object sender, EventArgs e)
		{
			if (currentPlayer == null || currentPlayer.IsBot()) return;
			int x = PointToClient(MousePosition).X * background.Width / ClientSize.Width;
			int y = PointToClient(MousePosition).Y * background.Height / ClientSize.Height;
			if (x <= 512)
			{
				selectedCardIndex = -1;
			}
			else if (x < 1396)	// main table
			{
				if (selectedCardIndex >= 0)
				{
					int ix = (x - 512 - 38) / 256;
					int iy = (y - 38) / 316;
					if (table.PlaceIsFree(ix, iy))
					{
						((LocalPlayer)player1).PlayerMove(selectedCardIndex, ix, iy);
					}
				}
				selectedCardIndex = -1;
			}
			else //user cards
			{
				int index = (y / 316) * 2 + ((x - 1396) / 256);
				if (index < player1.GetCards().Count)
				{
					selectedCardIndex = index;
				}
			}
			Refresh();
		}

		private void Form1_MouseDown(object sender, MouseEventArgs e)
		{
			if (currentPlayer == null || currentPlayer.IsBot()) return;
			int x = PointToClient(MousePosition).X * background.Width / ClientSize.Width;
			int y = PointToClient(MousePosition).Y * background.Height / ClientSize.Height;
			if (x > 1396)
			{
				int index = (y / 316) * 2 + ((x - 1396) / 256);
				if (index < player1.GetCards().Count)
				{
					selectedCardIndex = index;
					dragNDrop = true;
				}
			}
		}

		private void Form1_MouseUp(object sender, MouseEventArgs e)
		{
			if (currentPlayer == null || currentPlayer.IsBot()) return;
			if (!dragNDrop) return;
			int x = PointToClient(MousePosition).X * background.Width / ClientSize.Width;
			int y = PointToClient(MousePosition).Y * background.Height / ClientSize.Height;
			if (x > 512 && x < 1396)
			{
				if (selectedCardIndex >= 0)
				{
					int ix = (x - 512 - 38) / 256;
					int iy = (y - 38) / 316;
					if (table.PlaceIsFree(ix, iy))
					{
						((LocalPlayer)player1).PlayerMove(selectedCardIndex, ix, iy);
					}
				}
				selectedCardIndex = -1;
			}
			dragNDrop = false;
			Refresh();
		}

		private void DrawCard(Graphics canvas, bool enemy, Image card, Rectangle dest)
		{
			canvas.DrawImage((enemy) ? cardBackgroundEnemy : cardBackgroundPlayer, dest);
			canvas.DrawImage(card, dest);
		}

		private void Form1_Paint(object sender, PaintEventArgs e)
		{
			if (currentPlayer == null) return;
			Bitmap bitmap = new Bitmap(background);
			Graphics canvas = Graphics.FromImage(bitmap);
			//table cards
			for (int x = 0; x < 3; ++x)
			{
				for (int y = 0; y < 3; ++y)
				{
					if (table.PlaceIsFree(x, y)) continue;
					sTableCard tableCard = table.GetTableCardAt(x, y);
					Rectangle cardRect = new Rectangle(512 + 38 + 256 * tableCard.x, 38 + 316 * tableCard.y, 256, 316);
					if (tableCard.toBeFlipped)
					{
						if (animationPercent <= 50)
						{
							cardRect.X += 128 * animationPercent / 50;
							cardRect.Width -= 256 * animationPercent / 50;
						}
						else
						{
							cardRect.X += 128 * (100 - animationPercent) / 50;
							cardRect.Width -= 256 * (100 - animationPercent) / 50;
						}
					}
					DrawCard(canvas, tableCard.enemy, tableCard.card.image, cardRect);
				}
			}
			//enemy cards
			for (int i = 0; i < player2.GetCards().Count; ++i)
			{
				Rectangle destRect = new Rectangle((i % 2) * 256, i / 2 * 316, 256, 316);
				canvas.DrawImage(cardBackgroundBack, destRect);
			}
			//player cards
			List<sCard> playerCards = player1.GetCards();
			for (int i = 0; i < playerCards.Count; ++i)
			{
				Rectangle destRect = new Rectangle((i % 2) * 256 + 512 + 844, i / 2 * 316, 256, 316);
				if (i == selectedCardIndex)
				{
					if (dragNDrop)
					{
						int x = PointToClient(MousePosition).X * background.Width / ClientSize.Width;
						int y = PointToClient(MousePosition).Y * background.Height / ClientSize.Height;
						destRect = new Rectangle(x - 128, y - 158, 256, 316);
					}
					else
						destRect = new Rectangle((i % 2) * 256 + 512 + 844 - 10, i / 2 * 316 - 10, 256 + 20, 316 + 20);
				}
				DrawCard(canvas, false, playerCards[i].image, destRect);
			}
			canvas.Dispose();
			e.Graphics.DrawImage(bitmap, ClientRectangle);
			bitmap.Dispose();
		}

		private void Form1_MouseMove(object sender, MouseEventArgs e)
		{
			if (currentPlayer == null || currentPlayer.IsBot()) return;
			if (dragNDrop) Refresh();
		}

		private void SendData(sTableCard tcard)
		{
			if (!connection.Connected)
			{
				return;
			}
			byte[] bytes = new byte[13];
			BitConverter.GetBytes(cards.FindIndex(item => item.Equals(tcard.card))).CopyTo(bytes, 0);
			BitConverter.GetBytes(tcard.x).CopyTo(bytes, 4);
			BitConverter.GetBytes(tcard.y).CopyTo(bytes, 8);
			bytes[12] = Convert.ToByte(tcard.enemy);
			NetworkStream stream = connection.GetStream();
			stream.Write(bytes, 0, 13);
			stream.Flush();
		}

		private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			while (true)
			{
				if (backgroundWorker1.CancellationPending) return;
				while (currentPlayer == null || timer1.Enabled || table.GetNumberOfCards() == 9)
				{
					Thread.Sleep(10);
				}
				sTableCard tcard = currentPlayer.MakeMove(table);
				if (connection != null && currentPlayer.Equals(player1))
				{
					SendData(tcard);
				}
				if (tcard.card == null) continue;
				Invoke(new AddCardDelegate(AddCard), tcard);
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			NewGame();
			panel1.Visible = false;
			Refresh();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (connection != null) connection.Close();
			backgroundWorker1.CancelAsync();
			background.Dispose();
			cardBackgroundBack.Dispose();
			cardBackgroundEnemy.Dispose();
			cardBackgroundPlayer.Dispose();
			foreach (sCard card in cards)
			{
				card.image.Dispose();
			}
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			textBox1.Enabled = !textBox1.Enabled;
		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			panel2.Enabled = comboBox2.Text == "Network Player";
		}
	}
}