namespace RobotFight_WinForms
{
    partial class GameForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            saveFileDialog = new SaveFileDialog();
            openFileDialog = new OpenFileDialog();
            SuspendLayout();
            // 
            // saveFileDialog
            // 
            saveFileDialog.DefaultExt = "rf";
            saveFileDialog.Filter = "RobotFight match (*.rf)|*.rf";
            saveFileDialog.Title = "Saving RobotFight Game";
            // 
            // openFileDialog
            // 
            openFileDialog.Filter = "RobotFight match (*.rf)|*.rf";
            openFileDialog.Title = "Loading RobotFight Game";
            // 
            // GameForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(250, 282);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(2);
            MaximizeBox = false;
            Name = "GameForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Robot Fight";
            ResumeLayout(false);
        }

        #endregion

        private SaveFileDialog saveFileDialog;
        private OpenFileDialog openFileDialog;
    }
}
