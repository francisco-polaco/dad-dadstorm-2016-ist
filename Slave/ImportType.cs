namespace Slave
{
    public class FileImport : Import
    {
        private string[] filePaths;

        public FileImport(string[] filePaths)
        {
            this.filePaths = filePaths;
        }

        public void Import(string input)
        {
            // TO Do
        }

        

    }

    public class OpImport : Import
    {
        public void Import(string input)
        {
            // TO Do
        }
    }
}
