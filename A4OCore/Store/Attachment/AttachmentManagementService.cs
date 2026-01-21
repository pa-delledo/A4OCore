using A4ODto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace A4OCore.Store.Attachment
{

    public class AttachmentManagementService : IAttachmentManagementService
    {
        private readonly IFileService _fileService;
        private readonly ILogger<AttachmentManagementService> _logger;

        public AttachmentManagementService(
            IFileService fileService,
            ILogger<AttachmentManagementService> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        public async Task ManageAttachmentsAsync(

            ElementA4ODto element,
            DesignElementDto design)
        {
            string currentAttachments = element.Attachments;
            var result = new Dictionary<ElementValueA4ODto, string>();

            // 1. Identificare gli allegati attuali
            var currentPaths = ParseAttachments(currentAttachments);

            // 2. Identificare gli allegati nella richiesta
            Dictionary<ElementValueA4ODto, string> newAttachments = ExtractNewAttachments(element, design);

            // 3. Identificare gli allegati da eliminare
            var pathsToDelete = currentPaths.Except(newAttachments.Values).ToList();

            // 4. Eliminare fisicamente dal disco gli allegati rimossi
            await DeleteAttachmentsAsync(pathsToDelete);

            // 5. Spostare i nuovi file dalla cartella temporanea a quella permanente
            foreach (var kvp in newAttachments)
            {
                var item = kvp.Key;
                var path = kvp.Value;


                item.StringVal = path ?? string.Empty;


                if (item.StringVal.StartsWith("Temp/"))
                {
                    try
                    {
                        var permanentPath = await _fileService.MoveTempToPermanentAsync(path);
                        item.StringVal = permanentPath;
                        _logger.LogInformation("Allegato spostato da {TempPath} a {PermanentPath}", path, permanentPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Errore durante lo spostamento dell'allegato {Path}", path);
                        throw;
                    }
                }
                //else
                //{
                //    // Il file è già in posizione permanente
                //    item.StringVal = path;
                //}
            }


        }

        /// <summary>
        /// Estrae i percorsi degli allegati dalla stringa separata da virgole
        /// </summary>
        private HashSet<string> ParseAttachments(string attachments)
        {
            if (string.IsNullOrWhiteSpace(attachments))
                return new HashSet<string>();

            return new HashSet<string>(
                attachments.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(p => p.Trim())
                          .Where(p => !string.IsNullOrEmpty(p)));
        }

        /// <summary>
        /// Estrae gli allegati dai valori dell'elemento, filtrando solo i campi di tipo ATTACHMENT
        /// </summary>
        private Dictionary<ElementValueA4ODto, string> ExtractNewAttachments(
            ElementA4ODto element,
            DesignElementDto design)
        {
            var result = new Dictionary<ElementValueA4ODto, string>();

            if (element?.Values == null || design?.ItemsDesignBase == null)
                return result;

            // Crea un dizionario per cercare rapidamente i design items
            var designItemsByInfoData = design.ItemsDesignBase
                .Where(d => d.DesignType == ValueDesignType.ATTACHMENT)
                .ToDictionary(d => d.InfoData);

            foreach (var value in element.Values)
            {
                // Verifica se questo valore corrisponde a un campo attachment
                if (designItemsByInfoData.ContainsKey(value.InfoData))
                {
                    result[value] = value.StringVal ?? string.Empty;
                }
            }

            return result;
        }

        /// <summary>
        /// Elimina fisicamente i file specificati dal disco
        /// </summary>
        private async Task DeleteAttachmentsAsync(List<string> pathsToDelete)
        {
            if (pathsToDelete.Count == 0)
                return;

            await Task.Run(() =>
            {
                foreach (var path in pathsToDelete)
                {
                    try
                    {
                        var fullPath = Path.Combine(_fileService.GetStoragePath(), path);
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            _logger.LogInformation("Allegato eliminato: {Path}", path);

                            // Tenta di eliminare la cartella se vuota
                            var directory = Path.GetDirectoryName(fullPath);
                            if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
                            {
                                Directory.Delete(directory);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Errore durante l'eliminazione dell'allegato {Path}", path);
                    }
                }
            });
        }
    }
}
