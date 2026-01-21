using A4ODto;

namespace A4OCore.Store.Attachment
{
    public interface IAttachmentManagementService
    {
        /// <summary>
        /// Gestisce gli allegati durante il salvataggio di un elemento
        /// </summary>
        /// <param name="currentAttachments">Percorsi degli allegati attuali (separati da virgola)</param>
        /// <param name="element">Elemento con i nuovi valori</param>
        /// <param name="design">Design dell'elemento per identificare i campi attachment</param>
        /// <returns>Lista dei nuovi percorsi da salvare nel database</returns>
        Task ManageAttachmentsAsync(
            ElementA4ODto element,
            DesignElementDto design);
    }
}
