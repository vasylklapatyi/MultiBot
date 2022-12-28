﻿using Application.Services;
using Application.Services.Files;
using Application.Services.FileStorage;
using Application.TelegramBot.Pipelines.V2.Core.Context;
using Application.TelegramBot.Pipelines.V2.Core.Interfaces;
using Application.TextCommunication.Core.Repsonses;
using Application.TextCommunication.Core.Routing;
using Application.TextCommunication.Core.StageMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.TextCommunication.Core.Repsonses.Menu;

namespace Application.TelegramBot.Pipelines.V2.Pipelines.NotesV2;

[Route("/new-note", "📋 New note")]
public class AddNoteCommand : ITelegramCommand
{
    public void DefineStages(StageMapBuilder builder)
    {
        builder
            .Stage<SaveNoteStage>()
            .Stage<AcceptTagginNewNoteDesicion>()
            .Stage<TagNewlySavedNote>();
    }

    public Task<StageResult> Execute(TelegramMessageContext ctx)
    {
        return ContentResponse.Text("Enter Note text:");
    }
}
public class SaveNoteStage : ITelegramStage
{
    private readonly NoteAppService _noteService;
    private readonly FileStorageService _storageService;
    private readonly StoragePathService _pathService;
    public const string NOTEID_CACHEKEY = "NoteId";
    public SaveNoteStage(NoteAppService noteAppService,
        FileStorageService fileStorageService,
        StoragePathService storagePathService)
    {
        _noteService = noteAppService;
        _storageService = fileStorageService;
        _pathService = storagePathService;
    }
    public Task<StageResult> Execute(TelegramMessageContext ctx)
    {
        var files = ctx.Message.Files;
        Guid? id = null;
        bool hasMultipleFiles = false;
        if (files != null || files != null && files.Count != 0)
        {
            if (!files.All(f => f.UploadSucceeded))
            {
                ctx.Response.ForbidNextStageInvokation();
                return ContentResponse.Text("Sorry, these files can not be uploaded. Try again without them.");
            }
            foreach (var file in files)
            {
                hasMultipleFiles = true;
                var fileDto = _storageService.GetFileByHashAsync(ctx.User.Id, file.FileHash).Result;
                var newHash = _storageService.UploadFileAsync(ctx.User.Id, fileDto.Stream, $"{_pathService.GetNotesFolderName()}/{_pathService.GetFileName(fileDto.FilePath)}");
                id = _noteService.Create(ctx.User.Id, ctx.Message.Text, file.FileHash).Id;
            }
        }
        else
        {
            id = _noteService.Create(ctx.User.Id, ctx.Message.Text).Id;
        }
        if (!hasMultipleFiles)
            ctx.Cache.Set(NOTEID_CACHEKEY, id);

        return Task.FromResult(StageResult.ContentResult(new()
        {
            Text = hasMultipleFiles ? "✅ Note saved." : "✅ Note saved. Do you want to add tags the note?",
            Menu = hasMultipleFiles ? null : new(MenuType.MessageMenu, new[]
            {
                new[] {new Button("Yes", true.ToString()) },
                new[] {new Button("No", false.ToString()) }
            })
        }));
    }
}
public class AcceptTagginNewNoteDesicion : ITelegramStage
{
    private readonly TagAppService _tagService;
    public const string TAGDICTIONARY_CACHEKEY = "TagsDictionary";

    public AcceptTagginNewNoteDesicion(TagAppService tagAppService)
    {
        _tagService = tagAppService;
    }
    public Task<StageResult> Execute(TelegramMessageContext ctx)
    {
        var text = ctx.Message.Text;
        if (bool.TryParse(text, out var desicion))
        {
            if (desicion)
            {
                var tags = _tagService.GetAll(ctx.User.Id);

                var b = new StringBuilder();
                b.AppendLine("Enter the number of a tag or tags(separate them using comma) to select which tags you want to attach the note to:");

                int counter = 0;
                var dictionary = new Dictionary<int, Guid>();

                foreach (var item in tags)
                {
                    ++counter;
                    b.AppendLine($"🔷 {counter}. {item.Name}");
                    dictionary[counter] = item.Id;
                }

                ctx.Cache.Set(TAGDICTIONARY_CACHEKEY, dictionary);

                return ContentResponse.Text(b.ToString());
            }
            else
            {
                ctx.Response.SetPipelineEnded();
                return ContentResponse.Text("Ok");
            }
        }
        else
        {
            return ContentResponse.Text("Ok,move next");
        }
    }
}
public class TagNewlySavedNote : ITelegramStage
{
    private readonly TagAppService _tagService;
    private readonly NoteAppService _noteService;

    public TagNewlySavedNote(TagAppService tagAppService, NoteAppService noteAppService)
    {
        _tagService = tagAppService;
        _noteService = noteAppService;
    }
    public Task<StageResult> Execute(TelegramMessageContext ctx)
    {
        var noteId = ctx.Cache.Get<Guid>(SaveNoteStage.NOTEID_CACHEKEY);
        var text = ctx.Message.Text;
        var dict = ctx.Cache.Get<Dictionary<int, Guid>>(AcceptTagginNewNoteDesicion.TAGDICTIONARY_CACHEKEY);
        try
        {
            int[] numbers = GetValidated(text, 1, dict.Count);
            numbers
                .ToList()
                .ForEach(x =>
                {
                    var tagId = dict[x];
                    _noteService.TagNote(noteId, tagId);
                });
        }
        catch (IndexOutOfRangeException _)
        {
            ctx.Response.ForbidNextStageInvokation();
            return ContentResponse.Text("⛔️ Enter a number or numbers form the suggested list");
        }
        return ContentResponse.Text("🫡 Done");
    }

    /// <summary>
    /// Parses numbers from string
    /// </summary>
    /// <param name="input">inout string</param>
    /// <param name="minNumber"> min number of range</param>
    /// <param name="maxNumber"> max number of range</param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException"> is thrown if there is a number outside of the given range</exception>
    private int[] GetValidated(string input, int minNumber, int maxNumber)
    {
        int[] result = null;
        bool singleNumber = int.TryParse(input, out int rparsingResult);
        if (singleNumber)
        {
            result = new[] { rparsingResult };
        }
        else //if the string contains more than ont value - parse them
        {
            var preparatedString = input.Replace(" ", "");
            var matches = preparatedString
                                        .Split(",")
                                        .All(x => int.TryParse(x, out _));
            if (matches)
            {
                result = preparatedString
                    .Split(",")
                    .Select(x => int.Parse(x))
                    .ToArray();
            }
        }

        if (result != null)
        {
            //validate by range
            bool validatedByRange = result.All(n => n >= minNumber && n <= maxNumber);

            if (!validatedByRange)
                throw new IndexOutOfRangeException();
            //get distinct values
            result = result
                            .Distinct()
                            .ToArray();
        }
        return result;
    }
}