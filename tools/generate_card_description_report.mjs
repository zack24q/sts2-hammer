import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const repositoryRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const localizationPath = path.join(
  repositoryRoot,
  "HammerMod",
  "localization",
  "zhs",
  "cards.json",
);
const reportPath = path.join(repositoryRoot, "docs", "card-descriptions-zh.md");
const localization = JSON.parse(fs.readFileSync(localizationPath, "utf8"));

const titleEntries = Object.entries(localization).filter(([key]) => key.endsWith(".title"));
const lines = [
  "# 大锤猎手全部卡牌中文描述",
  "",
  `> 来源：\`HammerMod/localization/zhs/cards.json\` 当前工作区版本。共${titleEntries.length}张卡牌；以下保留动态变量和官方条件语法，便于逐句精简。`,
  "",
];

for (const [index, [titleKey, title]] of titleEntries.entries()) {
  const prefix = titleKey.slice(0, -".title".length);
  const description = localization[`${prefix}.description`];
  if (typeof description !== "string") {
    throw new Error(`Missing description for ${prefix}`);
  }

  lines.push(
    `## ${index + 1}. ${title}`,
    "",
    `- ID：\`${prefix}\``,
    "- 当前描述：",
    "",
    "```text",
    description,
    "```",
    "",
  );
}

const baseSuffixes = new Set(["title", "description", "smartDescription"]);
const extraEntries = Object.entries(localization).filter(([key]) => {
  const separator = key.lastIndexOf(".");
  return separator >= 0 && !baseSuffixes.has(key.slice(separator + 1));
});

if (extraEntries.length > 0) {
  lines.push(
    "# 卡面附加描述与选择提示",
    "",
    "以下文本不属于主`description`字段，但会在对应交互中显示。",
    "",
  );

  for (const [key, value] of extraEntries) {
    lines.push(
      `## ${key}`,
      "",
      "```text",
      value,
      "```",
      "",
    );
  }
}

fs.writeFileSync(reportPath, `${lines.join("\n").trimEnd()}\n`, "utf8");
