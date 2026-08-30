# For the record, I think ChatGPT did a great job with this.
# AI, especially for simple stupid stuff, is actually pretty good at making Python scripts.
# If you have some long tedious 9-to-5 desk work you'd rather not do, see how it works.

# This was on ChatGPT with the think mode on.
# Translations were not AI-generated, this just converts them to RESX.

"""
Fill a .resx localization file from a two-column translation spreadsheet.

Usage:
    python fill_resx.py translations.csv input.resx output.resx

The CSV is expected to have:
    Column A = Original (English)
    Column B = Translation

Special case:
    RESX key t30 is made from spreadsheet rows 47 and 48 (B47 + B48),
    separated by a single space.
"""

import argparse
import csv
import html
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


def read_translations(csv_path: Path) -> dict[str, str]:
    """Read English -> translated text from the first two CSV columns."""
    translations = {}

    with csv_path.open("r", encoding="utf-8-sig", newline="") as f:
        rows = list(csv.reader(f))

    if not rows:
        raise ValueError("The CSV file is empty.")

    # The supplied format has translations in rows 47 and 48 (1-based).
    if len(rows) < 48 or len(rows[46]) < 2 or len(rows[47]) < 2:
        raise ValueError("The CSV does not contain the expected rows 47 and 48.")

    # Normal mappings: use every non-empty English/translation pair.
    for row in rows:
        if len(row) < 2:
            continue

        english = row[0].strip()
        translation = row[1].strip()

        # Ignore blank rows and the spreadsheet's own header/instructions.
        if not english or not translation:
            continue
        if english == "Original (English)":
            continue
        if english.startswith("(Put your language's name"):
            continue

        if english in translations and translations[english] != translation:
            raise ValueError(
                f"Duplicate English text with different translations: {english!r}"
            )

        translations[english] = translation

    # Special case: RESX t30 is the combination of spreadsheet B47 + B48.
    part1 = rows[46][1].strip()
    part2 = rows[47][1].strip()
    translations["__T30_COMBINED__"] = f"{part1} {part2}"

    return translations


def load_resx(resx_path: Path) -> ET.ElementTree:
    """Parse the RESX first so malformed XML fails before writing anything."""
    return ET.parse(resx_path)


def replace_resx_values(resx_path: Path, output_path: Path,
                        translations: dict[str, str]) -> tuple[int, list[str]]:
    """
    Replace only <value> contents, preserving the rest of the RESX text
    (comments, formatting, headers, attributes, etc.).
    """
    text = resx_path.read_text(encoding="utf-8-sig")

    # Extract each <data ...>...</data> block and its name.
    data_pattern = re.compile(
        r'(<data\b[^>]*\bname="([^"]+)"[^>]*>.*?</data>)',
        re.DOTALL,
    )
    value_pattern = re.compile(
        r'(<value\b[^>]*>)(.*?)(</value>)',
        re.DOTALL,
    )

    replaced = 0
    missing = []

    def replace_data(match: re.Match) -> str:
        nonlocal replaced

        block = match.group(1)
        key = match.group(2)

        # Find the current English value from the XML block.
        value_match = value_pattern.search(block)
        if not value_match:
            missing.append(f"{key} (no <value> element)")
            return block

        current_value = html.unescape(value_match.group(2))

        # t30 is the one special spreadsheet-row mapping.
        if key == "t30":
            new_value = translations["__T30_COMBINED__"]
        elif current_value in translations:
            new_value = translations[current_value]
        else:
            missing.append(f"{key}: {current_value!r}")
            return block

        # Escape XML-sensitive characters while keeping Unicode readable.
        escaped = html.escape(new_value, quote=False)

        new_block = (
            block[:value_match.start(2)]
            + escaped
            + block[value_match.end(2):]
        )
        replaced += 1
        return new_block

    new_text = data_pattern.sub(replace_data, text)

    output_path.write_text(new_text, encoding="utf-8")
    return replaced, missing


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Fill a RESX localization file from a translation CSV."
    )
    parser.add_argument("csv", type=Path, help="Translation CSV file")
    parser.add_argument("resx", type=Path, help="English/source RESX file")
    parser.add_argument("output", type=Path, help="Output translated RESX file")
    args = parser.parse_args()

    for path in (args.csv, args.resx):
        if not path.is_file():
            print(f"Error: file not found: {path}", file=sys.stderr)
            return 1

    try:
        translations = read_translations(args.csv)
        load_resx(args.resx)  # Validate the input before changing anything.
        replaced, missing = replace_resx_values(
            args.resx, args.output, translations
        )
        load_resx(args.output)  # Validate the generated RESX too.
    except (OSError, ValueError, ET.ParseError) as exc:
        print(f"Error: {exc}", file=sys.stderr)
        return 1

    print(f"Created: {args.output}")
    print(f"Translated RESX entries: {replaced}")

    if missing:
        print("\nEntries not translated:")
        for item in missing:
            print(f"  - {item}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
