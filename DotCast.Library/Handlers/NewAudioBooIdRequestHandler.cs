using System.Globalization;
using System.Text;
using DotCast.Infrastructure.Persistence.Base.Repositories;
using DotCast.Library.Specifications;
using DotCast.SharedKernel.Messages;
using DotCast.SharedKernel.Models;

namespace DotCast.Library.Handlers
{
    public class NewAudioBooIdRequestHandler(IRepository<AudioBook> repository) : IMessageHandler<NewAudioBookIdRequest, string>
    {
        public async Task<string> Handle(NewAudioBookIdRequest message)
        {
            var id = GenerateId(message.Name);
            var exists = await repository.GetBySpecAsync(new AudioBookExistenceCheckSpecification(id));
            if (exists)
            {
                throw new ArgumentException($"AudioBook with id \"{id}\" already exists");
            }

            return id;
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