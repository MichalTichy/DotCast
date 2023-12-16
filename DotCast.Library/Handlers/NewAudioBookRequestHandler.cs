using System.Globalization;
using System.Text;
using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.Library.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class NewAudioBookRequestHandler(IRepository<AudioBook> repository) : MessageHandler<NewAudioBookRequest, string>
    {
        public override async Task<string> Handle(NewAudioBookRequest message)
        {
            if (message.AudioBook == null)
            {
                var id = GenerateId(message.Name);
                var exists = await repository.GetBySpecAsync(new AudioBookExistenceCheckSpecification(id));
                if (exists)
                {
                    throw new ArgumentException($"AudioBook with id \"{id}\" already exists");
                }

                return id;
            }

            if (message.Name != message.AudioBook.Name)
            {
                throw new ArgumentException($"Provided names do not match. \"{message.Name}\" != \"{message.AudioBook.Name}\"");
            }

            var existing = await repository.GetByIdAsync(message.AudioBook.Id);
            if (existing != null)
            {
                throw new ArgumentException($"AudioBook with id \"{message.AudioBook.Id}\" already exists");
            }

            await repository.AddAsync(message.AudioBook);

            return message.AudioBook.Id;
        }

        private string GenerateId(string name)
        {
            var normalizedName = name.ToLower().Replace(" ", "-");
            normalizedName = RemoveDiacritics(normalizedName);
            return normalizedName;
        }

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}