using System;
using Fluxor;
using RGS.Backend.Shared.Models;
using RGS.Frontend.Store.EditResumeDataFeature;

namespace RGS.Frontend.Store.EditResumeDataFeature;

public record struct RemoveBookAction(int BookIndex);
public record struct AddBookAction();
public record struct UpdateBookAction(int BookIndex, Book Book);

internal static class BooksReducers
{
  [ReducerMethod]
  public static EditResumeDataState RemoveBook(EditResumeDataState state, RemoveBookAction action)
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
  public static EditResumeDataState AddEducation(EditResumeDataState state, AddBookAction action)
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
  public static EditResumeDataState UpdateEducation(EditResumeDataState state, UpdateBookAction action)
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