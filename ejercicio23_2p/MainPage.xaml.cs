using System;
using Xamarin.Forms;
using Plugin.AudioRecorder;
using System.IO;
using Xamarin.Essentials;

namespace ejercicio23_2p
{
    public partial class MainPage : ContentPage
    {
        private readonly AudioRecorderService audioRecorderService = new AudioRecorderService();
        private readonly AudioPlayer audioPlayer = new AudioPlayer();
        public string AudioPath, fileName;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void btnGrabar_Clicked(object sender, EventArgs e)
        {
           
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            var status2 = await Permissions.RequestAsync<Permissions.StorageRead>();
            var status3 = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted & status2 != PermissionStatus.Granted & status3 != PermissionStatus.Granted)
            { 
                return; // si no tiene los permisos no avanza
            }


            if (audioRecorderService.IsRecording)
            {
                await audioRecorderService.StopRecording();
                audioPlayer.Play(audioRecorderService.GetAudioFilePath());
            }
            else
            {
                btnGrabar.IsVisible = false;
                btnDetener.IsVisible = true;
                await audioRecorderService.StartRecording();
            }
         
        }


        

        private async void btnDetener_Clicked(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(txtdescripcion.Text))
            {
                await DisplayAlert("Aviso", "Ingrese una descripción", "Hecho");
                return;
            }

            if (audioRecorderService.IsRecording)
            {
                await audioRecorderService.StopRecording();
                saveAudio();

                btnGrabar.IsVisible = true;
                btnDetener.IsVisible = false;

            }
            else
            {
                await audioRecorderService.StartRecording();
            }

        }


        private async void saveAudio()
        {
            if (audioRecorderService.FilePath != null) 
            {

                var stream = audioRecorderService.GetAudioFileStream();

                fileName = Path.Combine(FileSystem.CacheDirectory, DateTime.Now.ToString("ddMMyyyymmss") + "_VoiceNote.wav");

                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }

                AudioPath = fileName;

            }



            Models.AudioModel audio = new Models.AudioModel();

            audio.Url = AudioPath;
            audio.Descripcion = txtdescripcion.Text;
            audio.Date = DateTime.Now;

            var resultado = await App.BaseDatos.GrabarAudio(audio);

            if (resultado == 1)
            {
                await DisplayAlert("", "El audio " + audio.Descripcion + " se guardó correctamente en la ruta " + audio.Url, "Hecho");
                txtdescripcion.Text = "";
            }
            else
            {
                await DisplayAlert("Aviso", "No se pudo guardar el audio", "Hecho");
            }
        }

        private async void btnlistarAudio_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Views.ListarAudioPage());
        }
    }
}
