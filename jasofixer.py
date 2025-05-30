import os
import shutil
import unicodedata
import tkinter as tk
from tkinter import ttk, filedialog, messagebox
from datetime import datetime

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
    changed_logs = []

    for dirpath, dirnames, filenames in os.walk(root_dir, topdown=False):
        for filename in filenames:
            original_path = os.path.join(dirpath, filename)
            normalized_filename = normalize_path(filename)
            normalized_path = os.path.join(dirpath, normalized_filename)

            if original_path != normalized_path:
                try:
                    os.rename(original_path, normalized_path)
                    changed += 1
                    rel_path = os.path.relpath(original_path, root_dir)
                    log_entry = f"[FILE] {rel_path} → {normalized_filename}"
                    changed_logs.append(log_entry)
                    if log_callback:
                        log_callback(log_entry)
                except Exception as e:
                    err_msg = f"[FILE] Failed: {filename} - {e}"
                    changed_logs.append(err_msg)
                    if log_callback:
                        log_callback(err_msg)

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
                    rel_path = os.path.relpath(original_dir_path, root_dir)
                    log_entry = f"[DIR ] {rel_path} → {normalized_dirname}"
                    changed_logs.append(log_entry)
                    if log_callback:
                        log_callback(log_entry)
                except Exception as e:
                    err_msg = f"[DIR ] Failed: {dirname} - {e}"
                    changed_logs.append(err_msg)
                    if log_callback:
                        log_callback(err_msg)

            current += 1
            if progress_callback:
                progress_callback(current, total, changed)

    return changed_logs

def save_log_to_file(logs):
    now = datetime.now().strftime("%Y%m%d_%H%M%S")
    filename = f"변경_로그_{now}.txt"
    with open(filename, "w", encoding="utf-8") as f:
        for line in logs:
            f.write(line + "\n")
    return filename

def backup_folder(src):
    now = datetime.now().strftime("%Y%m%d_%H%M%S")
    dst = f"{src}_backup_{now}"
    try:
        shutil.copytree(src, dst)
        return dst
    except Exception as e:
        messagebox.showerror("백업 실패", f"백업 중 오류 발생:\n{e}")
        return None

def start_normalization():
    folder_selected = filedialog.askdirectory(title="정규화할 폴더를 선택하세요")

    if not folder_selected:
        messagebox.showwarning("취소됨", "폴더가 선택되지 않았습니다.")
        return

    if backup_var.get():
        backup_path = backup_folder(folder_selected)
        if backup_path:
            messagebox.showinfo("백업 완료", f"백업이 완료되었습니다:\n{backup_path}")
        else:
            return

    progress_bar["value"] = 0
    status_label.config(text="정규화 중...")
    log_text.delete(1.0, tk.END)

    def update_progress(current, total, changed):
        percent = int((current / total) * 100)
        progress_bar["value"] = percent
        progress_label.config(text=f"{current}/{total} 처리됨 • {changed}개 변경됨")
        root.update_idletasks()

    def log_change(message):
        log_text.insert(tk.END, message + "\n")
        log_text.see(tk.END)

    changed_logs = normalize_filenames_recursively(
        folder_selected,
        progress_callback=update_progress,
        log_callback=log_change
    )

    progress_bar["value"] = 100
    status_label.config(text="정규화 완료!")

    if changed_logs and log_save_var.get():
        log_file = save_log_to_file(changed_logs)
        messagebox.showinfo("완료", f"정규화 완료!\n로그 저장됨:\n{log_file}")
    elif changed_logs:
        messagebox.showinfo("완료", "정규화 완료!\n로그는 저장되지 않았습니다.")
    else:
        messagebox.showinfo("완료", "정규화 완료!\n변경된 항목은 없습니다.")

# GUI 구성
root = tk.Tk()
root.title("한글 자소 정규화 도구")
root.geometry("620x500")
root.resizable(False, False)

# 프레임: 실행 버튼과 체크박스 수평 정렬
action_frame = tk.Frame(root)
action_frame.pack(pady=15)

# 실행 버튼 (크게)
start_btn = tk.Button(action_frame, text="폴더 선택 및 정규화 시작", command=start_normalization, width=30, height=2)
start_btn.pack(side=tk.LEFT, padx=10)

# 체크박스들
backup_var = tk.BooleanVar(value=True)
log_save_var = tk.BooleanVar(value=True)

check_frame = tk.Frame(action_frame)
check_frame.pack(side=tk.LEFT, padx=10)

tk.Checkbutton(check_frame, text="정규화 전에 백업", variable=backup_var).pack(anchor='w')
tk.Checkbutton(check_frame, text="변경 로그 파일 저장", variable=log_save_var).pack(anchor='w')

# 진행 바
progress_bar = ttk.Progressbar(root, length=550)
progress_bar.pack(pady=5)

progress_label = tk.Label(root, text="0/0 처리됨 • 0개 변경됨")
progress_label.pack()

status_label = tk.Label(root, text="폴더를 선택하세요")
status_label.pack(pady=5)

# 로그 창
log_frame = tk.Frame(root)
log_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

scrollbar = tk.Scrollbar(log_frame)
scrollbar.pack(side=tk.RIGHT, fill=tk.Y)

log_text = tk.Text(log_frame, height=10, wrap=tk.WORD, yscrollcommand=scrollbar.set)
log_text.pack(fill=tk.BOTH, expand=True)
scrollbar.config(command=log_text.yview)

root.mainloop()
