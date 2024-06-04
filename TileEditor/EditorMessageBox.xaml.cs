using System.Windows;
using System.Windows.Controls;

namespace TileEditor;

public enum CustomDialogResult
{
    OK,
    CANCEL,
    NONE
}

public enum Buttons
{
    OK,
    OK_CANCEL
}

public partial class EditorMessageBox : Window
{
    private CustomDialogResult _result = CustomDialogResult.NONE;

    public EditorMessageBox(string message, string title, Buttons buttons)
    {
        InitializeComponent();
        Owner = Application.Current.MainWindow;

        this.Title = title;

        var messageTextBlock = new TextBlock()
        {
            Text = message,
            TextAlignment = TextAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 16,
            TextWrapping = TextWrapping.Wrap,
        };
        this.content.Children.Add(messageTextBlock);

        var buttonRow = CreateButtons(buttons);
        for (int i = 0; i < buttonRow.Length; i++)
        {
            this.buttons.ColumnDefinitions.Add(new ColumnDefinition());
            var border = new Border()
            {
                Padding = new Thickness(5),
                Child = buttonRow[i]
            };
            this.buttons.Children.Add(border);
            Grid.SetColumn(border, i);
        }
    }

    public new CustomDialogResult ShowDialog()
    {
        base.ShowDialog();
        return _result;
    }

    private Button[] CreateButtons(Buttons buttons)
    {
        return buttons switch
        {
            Buttons.OK_CANCEL => CreateOkCancelButtons(),
            Buttons.OK => CreateOkButton(),
            _ => throw new InvalidOperationException(),
        };
    }

    private Button[] CreateOkCancelButtons()
    {
        var okButton = new Button
        {
            Content = "OK"
        };
        okButton.Click += (_, _) =>
        {
            _result = CustomDialogResult.OK;
            Close();
        };

        var cancelButton = new Button
        {
            Content = "Cancel"
        };
        cancelButton.Click += (_, _) =>
        {
            _result = CustomDialogResult.CANCEL;
            Close();
        };

        return [okButton, cancelButton];
    }

    private Button[] CreateOkButton()
    {
        var okButton = new Button
        {
            Content = "OK"
        };
        okButton.Click += (_, _) =>
        {
            _result = CustomDialogResult.OK;
            Close();
        };

        return [okButton];
    }

}
