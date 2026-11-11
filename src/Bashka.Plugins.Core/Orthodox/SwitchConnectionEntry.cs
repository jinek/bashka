using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;

namespace Bashka.Plugins.Core.Orthodox
{
    public class SwitchConnectionEntry : Entry
    {
        public Func<IShellConnection> ConnectionFactory { get; }

        public SwitchConnectionEntry(string name, ICatalog catalog, Func<IShellConnection> connectionFactory) : base(
            (string.Empty, name, -1, DateTime.Now, EntryCategory.Default), false, catalog)
        {
            ConnectionFactory = connectionFactory;
        }
    }
}