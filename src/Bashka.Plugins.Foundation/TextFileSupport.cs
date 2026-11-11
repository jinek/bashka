namespace Bashka.Plugins.Foundation;

public static class TextFileSupport
{
    public const string SupportedExtensions =
        // Documents & Text
        @"\.(txt|md|markdown|mdown|mkd|rst|log|tex|adoc|asciidoc)$" +
        // Data & Config
        @"|\.(json|jsonc|json5|geojson|topojson|yaml|yml|toml|ini|cfg|conf|config|properties|env|csv|tsv|lock)$" +
        // Markup & Web
        @"|\.(xml|xaml|axaml|xsd|xsl|xslt|svg|plist|html|htm|xhtml|css|scss|sass|less|vue|svelte|asp|aspx|jsp)$" +
        // Programming & Scripts
        @"|\.(c|cpp|cxx|cc|h|hpp|hxx|hh|inl|inc|cs|csx|fs|fsi|fsx|vb|java|kt|kts|scala|groovy|gradle)$" +
        @"|\.(js|jsx|mjs|cjs|ts|tsx|mts|cts|php|py|pyw|pyi|ipynb|go|rs|swift|dart|zig|sql)$" +
        @"|\.(sh|bash|zsh|fish|ksh|csh|tcsh|bat|cmd|ps1|psm1|psd1|vbs|rb|erb|gemspec|rake|pl|pm|t|pod|lua|r|rmd)$" +
        @"|\.(asm|s|clj|cljs|cljc|edn|lisp|lsp|cl|scm|ss|erl|hrl|ex|exs|hs|lhs|ml|mli|proto|thrift|graphql|gql|diff|patch|cmake)$" +
        // Dotfiles & specific files
        @"|\.(dockerignore|gitignore|gitattributes|editorconfig|npmignore|prettierrc|eslintrc)$" +
        @"|(^|[\\/])(Dockerfile|Makefile|GNUmakefile|Containerfile|Vagrantfile|Rakefile|Gemfile|Procfile|CMakeLists\.txt|LICENSE|LICENCE|README|CHANGELOG|AUTHORS|NOTICE)(\.[a-zA-Z0-9]+)?$";
}
