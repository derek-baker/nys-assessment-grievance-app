export interface IBrowserStorage {
    setItem: (data: string, key: string) => void;
    getItem: (key: string) => string;
    removeItem: (key: string) => void;
    clear: () => void;
    length: number;
    key: (index: number) => string;
}
