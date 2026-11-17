# Pick Up, Traveler
---
## 프로젝트 개요
- **장르**: 비주얼노벨, 시뮬레이션
- **플랫폼**: PC (Windows)
- **개발 환경**: Unity (6000.0.44f1), C#
- **개발 기간**: 2025.10 ~ 2026.05
---
## 프리뷰
| 타이틀 화면 |
<img width="1337" height="780" alt="스크린샷 2026-09-03 221236" src="https://github.com/user-attachments/assets/1de250b7-d95c-4fe7-8069-66d839b1b247" />
| 맵 화면 |
<img width="1337" height="777" alt="스크린샷 2026-09-05 231853" src="https://github.com/user-attachments/assets/f787e011-d3ea-471c-83cf-a80d76d35fc4" />
| 크레인 게임 |
<img width="1337" height="776" alt="스크린샷 2026-09-05 232020" src="https://github.com/user-attachments/assets/280ef2d3-b8cf-4866-b944-efab1f000a26" />
| 상점 상호작용 |
<img width="1331" height="775" alt="스크린샷 2026-09-05 232043" src="https://github.com/user-attachments/assets/7b83f403-238b-42ff-8af7-1ebeb5680450" />
| 비주얼 노벨 UI |
<img width="1338" height="778" alt="스크린샷 2026-09-05 232203" src="https://github.com/user-attachments/assets/87fe6e45-5eac-468d-9fcb-ac53c6b02dc6" />
---
## 핵심 기능 및 특징

### 1. [핵심 기능 1: 인터랙티브 스토리텔링]
- 플레이어 선택지에 따른 멀티 엔딩 분기 구조 구현

### 2. [핵심 기능 2: 익숙한 크레인 게임 기믹]
- 일본 크레인 게임에서 가져온 기믹
- 크레인으로 상품을 들어올리고 방향을 돌려가며 아래로 떨어뜨리는 일련의 과정을 물리적으로 구현
---
## 기술 스택 및 아키텍처
- **Engine & Language**: Unity, C#
- **Design Pattern**: Singleton
- **Asset / Tools**: 유니티 에셋 스토어 [2D Casual UI / Arcades Pack 02 / InteriorAsset / POLYGON city pack], 폰트 [Thaleah_PixelFont], 사용한 AI 모델 [Google Gemini]
---
## 프로젝트 구조
```text
Assets/
├── Scripts/            # 스크립트
├── Material/           # 머터리얼
├── Resources/
│   ├── Audio/          # 오디오 파일
│   ├── Img/            # 이미지 파일
└   └── TravelData/     # 날짜별 비주얼노벨 데이터
