namespace PintagAPP.Presentacion.Utilidades
{
    // Tomar o elegir una foto, ya reducida (1024 px, calidad 80). Devuelve null si el usuario cancela.
    public static class SelectorFotos
    {
        private static MediaPickerOptions Opciones(string titulo) => new()
        {
            Title = titulo,
            MaximumWidth = 1024,
            MaximumHeight = 1024,
            CompressionQuality = 80
        };

        public static async Task<byte[]?> TomarAsync(string titulo)
        {
            if (!MediaPicker.Default.IsCaptureSupported)
                throw new InvalidOperationException("Este dispositivo no tiene cámara disponible.");

            var permiso = await Permissions.RequestAsync<Permissions.Camera>();
            if (permiso != PermissionStatus.Granted)
                throw new InvalidOperationException("Necesitamos permiso de cámara para tomar la foto.");

            return await LeerAsync(await MediaPicker.Default.CapturePhotoAsync(Opciones(titulo)));
        }

        public static async Task<byte[]?> ElegirAsync(string titulo)
        {
            return await LeerAsync(await MediaPicker.Default.PickPhotoAsync(Opciones(titulo)));
        }

        private static async Task<byte[]?> LeerAsync(FileResult? archivo)
        {
            if (archivo is null) return null;
            using var stream = await archivo.OpenReadAsync();
            using var memoria = new MemoryStream();
            await stream.CopyToAsync(memoria);
            return memoria.ToArray();
        }
    }
}
