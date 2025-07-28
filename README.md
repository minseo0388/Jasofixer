# Jasofixer (한글 자소 정규화 도구) v2.0

한글 파일명의 자소 분리 문제를 해결하는 Windows Forms 애플리케이션입니다.

## 주요 기능

- **한글 자소 정규화**: NFC(Canonical Decomposition, followed by Canonical Composition) 형식으로 파일 및 폴더명 정규화
- **백업 기능**: 정규화 작업 전 자동 백업 생성 (선택사항)
- **로그 기능**: 변경 내역을 타임스탬프와 함께 텍스트 파일로 저장
- **실시간 진행률**: 작업 진행 상황을 실시간으로 표시
- **작업 취소**: 진행 중인 작업을 안전하게 취소 가능
- **안전한 처리**: 중복 파일명 처리, 권한 오류 처리, 포괄적인 예외 처리
- **비동기 처리**: UI가 멈추지 않도록 백그라운드에서 작업 수행

## 시스템 요구사항

- Windows 7 이상
- .NET 6.0 Runtime 또는 SDK

## 빌드 방법

### 자동 빌드 (권장)
```bash
.\build.bat
```

### 수동 빌드
```bash
dotnet restore HangulJasofixer.csproj
dotnet build HangulJasofixer.csproj --configuration Release
```

## 실행 방법

### 자동 실행 (권장)
```bash
.\run.bat
```

### 수동 실행
빌드된 실행 파일 직접 실행:
```bash
.\bin\Release\net6.0-windows\HangulJasofixer.exe
```

또는 개발 모드로 실행:
```bash
dotnet run --project HangulJasofixer.csproj
```

## 사용법

1. 프로그램을 실행합니다
2. "폴더 선택 및 정규화 시작" 버튼을 클릭합니다
3. 정규화할 폴더를 선택합니다
4. 옵션을 확인합니다:
   - ✅ **정규화 전에 백업**: 작업 전 백업 폴더 생성 (기본값: 체크됨)
   - ✅ **변경 로그 파일 저장**: 변경 내역을 텍스트 파일로 저장 (기본값: 체크됨)
5. 작업이 시작되면 실시간으로 진행 상황을 확인할 수 있습니다
6. 필요시 "취소" 버튼을 클릭하여 작업을 중단할 수 있습니다
7. 작업이 완료될 때까지 기다립니다

## v2.0 개선사항

### 🔧 **코드 품질 개선**
- Nullable Reference Types 완전 지원
- 모든 컴파일러 경고 해결
- 스레드 안전성 개선 (`BeginInvoke` 사용)
- 메모리 누수 방지를 위한 적절한 리소스 해제

### 🚀 **성능 최적화**
- 비동기 작업 처리로 UI 응답성 향상
- 불필요한 `Application.DoEvents()` 호출 제거
- 효율적인 진행률 업데이트

### 🛡️ **안정성 강화**
- 포괄적인 예외 처리 및 오류 메시지
- 작업 취소 지원 (`CancellationToken` 사용)
- 입력 유효성 검사 강화
- 파일 접근 권한 오류 처리

### 🎨 **사용자 경험 개선**
- 취소 버튼 추가로 작업 제어 가능
- 타임스탬프가 포함된 로그 메시지
- 더 명확한 상태 표시
- 향상된 오류 알림

### 🔒 **보안 향상**
- 안전한 파일 경로 처리
- Null 참조 방지
- 적절한 예외 전파

## 프로젝트 구조

```
Jasofixer/
├── MainForm.cs          # 메인 UI 폼 (비동기 처리, 취소 지원)
├── Normalizer.cs        # 파일명 정규화 로직 (스레드 안전)
├── BackupManager.cs     # 백업 기능 (오류 처리 강화)
├── LogManager.cs        # 로그 저장 기능 (타임스탬프 포함)
├── Program.cs           # 애플리케이션 진입점 (전역 예외 처리)
├── build.bat           # 자동 빌드 스크립트
├── run.bat            # 자동 실행 스크립트
└── HangulJasofixer.csproj  # 프로젝트 설정
```

## 기술적 세부사항

- **Framework**: .NET 6.0 Windows Forms
- **언어**: C# 10.0 with nullable reference types
- **아키텍처**: Single-threaded UI with async/await pattern
- **정규화 방식**: Unicode NFC (Normalization Form Composed)
- **스레드 모델**: UI thread + background worker threads
- **취소 지원**: CancellationToken을 통한 협력적 취소

## 문제 해결

### 빌드 오류
- .NET 6.0 SDK가 설치되어 있는지 확인하세요
- `dotnet --version` 명령으로 설치 상태를 확인할 수 있습니다

### 실행 오류
- .NET 6.0 Runtime이 설치되어 있는지 확인하세요
- Windows 7의 경우 최신 업데이트가 필요할 수 있습니다

### 권한 오류
- 관리자 권한으로 실행해보세요
- 대상 폴더에 쓰기 권한이 있는지 확인하세요

## 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다. 자세한 내용은 `LICENSE` 파일을 참조하세요.

## 기여

버그 리포트, 기능 제안, 풀 리퀘스트를 환영합니다!

---

**개발자**: Choi Minseo  
**버전**: 2.0.0  
**최종 업데이트**: 2025-07-28
