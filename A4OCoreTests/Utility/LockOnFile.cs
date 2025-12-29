using System;
using System.IO;
using System.Threading;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.IO;
using System.Threading;

public class LockOnFile
{
    private readonly string _filePath;
    private readonly object _writeLock = new object();
    private readonly string _lockFileName = ".lockfile";  // Nome file di lock (nascosto)

    public LockOnFile(string filePath)
    {
        _filePath = filePath;
    }

    // Verifica se il file di lock esiste
    public bool IsLocked()
    {
        return File.Exists(_filePath + _lockFileName);
    }

    // Tenta di acquisire un lock di lettura sul file
    public bool TryLockRead()
    {
        try
        {
            // Se il file è già bloccato (esiste il file di lock), ritorna false
            if (IsLocked())
            {
                return false; // Lock già in uso
            }

            // Crea un file di lock per segnalare il lock in scrittura
            File.Create(_filePath + _lockFileName).Dispose();  // Il file di lock verrà creato, senza bloccare i thread di lettura
            return true;  // Lock di lettura acquisito
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore durante l'acquisizione del lock di lettura: {ex.Message}");
            return false;
        }
    }

    // Rilascia il lock di lettura
    public void UnlockRead()
    {
        try
        {
            if (IsLocked())
            {
                File.Delete(_filePath + _lockFileName); // Rimuove il file di lock
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore durante il rilascio del lock di lettura: {ex.Message}");
        }
    }

    // Metodo per tentare di acquisire un lock di scrittura esclusivo (unico processo o thread)
    public bool TryLockWrite()
    {
        lock (_writeLock)
        {
            try
            {
                // Se il file di lock esiste, il lock di scrittura è occupato
                if (IsLocked())
                {
                    return false; // Lock di scrittura già acquisito
                }

                // Crea il file di lock per segnalare che il lock di scrittura è stato acquisito
                File.Create(_filePath + _lockFileName).Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'acquisizione del lock di scrittura: {ex.Message}");
                return false;
            }
        }
    }

    // Rilascia il lock di scrittura
    public void UnlockWrite()
    {
        lock (_writeLock)
        {
            try
            {
                if (IsLocked())
                {
                    File.Delete(_filePath + _lockFileName); // Rimuove il file di lock
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il rilascio del lock di scrittura: {ex.Message}");
            }
        }
    }

    // Metodo che blocca il thread finché non acquisisce un lock di lettura
    public void WaitForReadLock()
    {
        while (true)
        {
            if (TryLockRead())
            {
                Console.WriteLine("Lock di lettura acquisito.");
                return;  // Lock ottenuto, esci dal ciclo
            }
            else
            {
                Console.WriteLine("File già bloccato per scrittura. Attendere...");
                Thread.Sleep(1000);  // Attende 1 secondo prima di ritentare
            }
        }
    }

    // Metodo che blocca il thread finché non acquisisce un lock di scrittura
    public void WaitForWriteLock()
    {
        while (true)
        {
            if (TryLockWrite())
            {
                Console.WriteLine("Lock di scrittura acquisito.");
                return;  // Lock ottenuto, esci dal ciclo
            }
            else
            {
                Console.WriteLine("File già bloccato. Attendere...");
                Thread.Sleep(1000);  // Attende 1 secondo prima di ritentare
            }
        }
    }
}
