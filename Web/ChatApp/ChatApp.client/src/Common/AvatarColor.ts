export function getAvatarColor(letter?: string): string {
    if (!letter) return '#000000';

    let hash = 0;
    for (let i = 0; i < letter.length; i++) {
        hash = letter.charCodeAt(i) + ((hash << 13) - hash);
    }

    const color = ((hash & 0x00FFFFFF) >>> 0).toString(16);
    return `#${'000000'.substring(0, 6 - color.length) + color}`;
}
