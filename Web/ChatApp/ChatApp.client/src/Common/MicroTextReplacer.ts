function replaceText(
  text: string,
  data: Record<string, string | number | Date>
): string {
  return text.replace(/\{\{(\w+)\}\}/g, (match, key) => {
    return key in data ? String(data[key]) : match;
  });
}

const data: Record<string, string | number | Date> = {
  NOW: new Date().toLocaleString(),
  NOW_DATA: new Date().toDateString(),
  NOW_TIME: new Date().toLocaleTimeString(),
};

export function MicroTextReplacer(text: string) {
  return replaceText(text, data);
}
