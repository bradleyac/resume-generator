using System;
using Fluxor;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.EditSourceResumeDataFeature;

public record struct RemoveBookAction(int BookIndex);
public record struct AddBookAction();
public record struct UpdateBookAction(int BookIndex, Book Book);

internal static class BooksReducers
{
  [ReducerMethod]
  public static EditSourceResumeDataState RemoveBook(EditSourceResumeDataState state, RemoveBookAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Bookshelf = [.. state.ResumeData.Bookshelf.RemoveAt(action.BookIndex)]
      }
    };
  }

  [ReducerMethod]
  public static EditSourceResumeDataState AddEducation(EditSourceResumeDataState state, AddBookAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Bookshelf = [.. state.ResumeData.Bookshelf, new("", "")],
      }
    };
  }

  [ReducerMethod]
  public static EditSourceResumeDataState UpdateEducation(EditSourceResumeDataState state, UpdateBookAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Bookshelf = [.. state.ResumeData.Bookshelf.ReplaceAt(action.BookIndex, _ => action.Book)],
      }
    };
  }
}