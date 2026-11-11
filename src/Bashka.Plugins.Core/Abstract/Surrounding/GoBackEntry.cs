using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.Abstract.Surrounding;

public class GoBackEntry(ICatalog catalog)
    : Entry(("..", "..", 0, DateTime.Now, EntryCategory.Default), false, catalog);