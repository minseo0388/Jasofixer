import os
import unicodedata
import tkinter as tk
from tkinter import ttk, filedialog, messagebox

def normalize_path(path):
    return unicodedata.normalize('NFC', path)

def count_total_items(root_dir):
    count = 0
    for dirpath, dirnames, filenames in os.walk(root_dir):
        count += len(filenames) + len(dirnames)
    return count

def normalize_filenames_recursively(root_dir, progress_callback=None, log_callback=None):
    total = count_total_items(root_dir)
    current = 0
    changed = 0

    for dirpath, dirnames, filenames in os.walk(root_dir, topdown=False):
        for filename in filenames:
            original_path = os.path.join(dirpath, filename)
            normalized_filename = normalize_path(filename)
            normalized_path = os.path.join(dirpath, normalized_filename)

            if original_path != normalized_path:
                try:
                    os.rename(original_path, normalized_path)
                    changed += 1
                    if log_callback:
                        rel_path = os.path.relpath(original_path, root_dir)
                        log_callback(f"[FILE] {rel_path} → {normalized_filename}")
                except Exception as e:
                    if log_callback:
                        log_callback(f"[FILE] Failed: {filename} - {e}")

            current += 1
            if progress_callback:
                progress_callback(current, total, changed)

        for dirname in dirnames:
            original_dir_path = os.path.join(dirpath, dirname)
            normalized_dirname = normalize_path(dirname)
            normalized_dir_path = os.path.join(dirpath, normalized_dirname)

            if original_dir_path != normalized_dir_path:
                try:
                    os.rename(original_dir_path, normalized_dir_path)
                    changed += 1
                    if log_callback:
                        rel_path = os.path.relpath(original_dir_path, root_dir)
                        log_callback(f"[DIR ] {rel_path} → {normalized_dirname}")
                except Exception as e:
                    if log_callback:
                        log_callback(f"[DIR ] Failed: {dirname} - {e}")

            current += 1
            if progress_callback:
                progress_callback(current, total, changed)

def start_normalization():
    folder_selected = filedialog.askdirectory(title="정규화할 폴더를 선택하세요")

    if not folder_selected:
        messagebox.showwarning("취소됨", "폴더가 선택되지 않았습니다.")
        return

    progress_bar["value"] = 0
    status_label.config(text="정규화 중...")
    log_text.delete(1.0, tk.END)  # 로그 초기화

    def update_progress(current, total, changed):
        percent = int((current / total) * 100)
        progress_bar["value"] = percent
        progress_label.config(text=f"{current}/{total} 처리됨 • {changed}개 변경됨")
        root.update_idletasks()

    def log_change(message):
        log_text.insert(tk.END, message + "\n")
        log_text.see(tk.END)

    normalize_filenames_recursively(folder_selected, progress_callback=update_progress, log_callback=log_change)

    progress_bar["value"] = 100
    status_label.config(text="정규화 완료!")
    messagebox.showinfo("완료", "자소 정규화가 완료되었습니다.")

# GUI
root = tk.Tk()
root.title("한글 자소 정규화 도구")
root.geometry("600x450")
root.resizable(False, False)

select_button = tk.Button(root, text="폴더 선택 및 정규화 시작", command=start_normalization)
select_button.pack(pady=10)

progress_bar = ttk.Progressbar(root, length=550)
progress_bar.pack(pady=5)

progress_label = tk.Label(root, text="0/0 처리됨 • 0개 변경됨")
progress_label.pack()

status_label = tk.Label(root, text="폴더를 선택하세요")
status_label.pack(pady=5)

log_frame = tk.Frame(root)
log_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

scrollbar = tk.Scrollbar(log_frame)
scrollbar.pack(side=tk.RIGHT, fill=tk.Y)

log_text = tk.Text(log_frame, height=10, wrap=tk.WORD, yscrollcommand=scrollbar.set)
log_text.pack(fill=tk.BOTH, expand=True)
scrollbar.config(command=log_text.yview)

root.mainloop()
