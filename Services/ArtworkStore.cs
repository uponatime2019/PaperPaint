using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PaperPaint.Helpers;
using PaperPaint.Models;
using Newtonsoft.Json;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PaperPaint.Services
{
    /// <summary>Local artwork library: PNG + thumbnail per painting plus a JSON index, all under LocalAppData\PaperPaint\Artwork.</summary>
    public static class ArtworkStore
    {
        private const string IndexName = "index.json";

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        public static async Task<StorageFolder> GetRootAsync()
        {
            string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PaperPaint",
                "Artwork");
            Directory.CreateDirectory(folderPath);
            return await StorageFolder.GetFolderFromPathAsync(folderPath);
        }

        public static async Task<List<ArtworkItem>> LoadIndexAsync()
        {
            try
            {
                var root = await GetRootAsync();
                var file = await root.TryGetItemAsync(IndexName) as StorageFile;
                if (file == null)
                {
                    return new List<ArtworkItem>();
                }

                using var stream = await file.OpenStreamForReadAsync();
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                var items = JsonConvert.DeserializeObject<List<ArtworkItem>>(json);
                return items ?? new List<ArtworkItem>();
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "ArtworkStore.LoadIndex");
                return new List<ArtworkItem>();
            }
        }

        public static async Task SaveIndexAsync(List<ArtworkItem> items)
        {
            var root = await GetRootAsync();
            var file = await root.CreateFileAsync(IndexName, CreationCollisionOption.ReplaceExisting);
            using var stream = await file.OpenStreamForWriteAsync();
            using var writer = new StreamWriter(stream);
            await writer.WriteAsync(JsonConvert.SerializeObject(items, JsonSettings));
        }

        /// <summary>Writes the PNG + thumbnail and upserts the index. Pass an existing item to overwrite it.</summary>
        public static async Task<ArtworkItem> SaveArtworkAsync(byte[] png, byte[] thumbPng, ArtworkItem? existing)
        {
            var root = await GetRootAsync();
            var items = await LoadIndexAsync();

            string id = existing?.Id ?? Guid.NewGuid().ToString("N");
            string fileName = $"{id}.png";
            string thumbName = $"{id}_thumb.png";

            await FileIO.WriteBytesAsync(await root.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting), png);
            await FileIO.WriteBytesAsync(await root.CreateFileAsync(thumbName, CreationCollisionOption.ReplaceExisting), thumbPng);

            var now = DateTime.UtcNow;
            ArtworkItem item;
            if (existing != null)
            {
                item = existing;
                var idx = items.FindIndex(x => x.Id == id);
                if (idx >= 0)
                {
                    items[idx] = item;
                }
                else
                {
                    items.Add(item);
                }
            }
            else
            {
                item = new ArtworkItem
                {
                    Id = id,
                    Name = await NextDefaultNameAsync(items),
                    File = fileName,
                    Thumb = thumbName,
                    CreatedUtc = now
                };
                items.Add(item);
            }

            item.File = fileName;
            item.Thumb = thumbName;
            item.ModifiedUtc = now;

            await SaveIndexAsync(items);
            return item;
        }

        private static Task<string> NextDefaultNameAsync(List<ArtworkItem> items)
        {
            int n = items.Count + 1;
            while (items.Any(x => x.Name == $"Painting {n}"))
            {
                n++;
            }

            return Task.FromResult($"Painting {n}");
        }

        public static async Task RenameAsync(ArtworkItem item, string newName)
        {
            var items = await LoadIndexAsync();
            var match = items.FirstOrDefault(x => x.Id == item.Id);
            if (match != null)
            {
                match.Name = newName;
                item.Name = newName;
                await SaveIndexAsync(items);
            }
        }

        public static async Task DeleteAsync(ArtworkItem item)
        {
            var root = await GetRootAsync();
            var items = await LoadIndexAsync();
            items.RemoveAll(x => x.Id == item.Id);
            await SaveIndexAsync(items);

            var file = await root.TryGetItemAsync(item.File);
            if (file != null)
            {
                await file.DeleteAsync();
            }

            var thumb = await root.TryGetItemAsync(item.Thumb);
            if (thumb != null)
            {
                await thumb.DeleteAsync();
            }
        }

        public static async Task<StorageFile> GetImageFileAsync(ArtworkItem item)
        {
            var root = await GetRootAsync();
            return await root.GetFileAsync(item.File);
        }

        public static async Task<IRandomAccessStream?> OpenThumbnailAsync(ArtworkItem item)
        {
            try
            {
                var root = await GetRootAsync();
                var file = await root.TryGetItemAsync(item.Thumb) as StorageFile;
                if (file == null)
                {
                    return null;
                }

                return await file.OpenReadAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
