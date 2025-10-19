# To a World
## Project Setup

- Required Assets (from Unity Asset Store):

1. [Magica Cloth 2](https://assetstore.unity.com/packages/tools/physics/magica-cloth-2-242307)

Please make sure to **download and import all assets** before running the project. These assets are required for proper scene rendering and interaction.


## <br>프로젝트 개요
To a World는 VR 기기를 사용하여 사용자가 가상의 세계여행을 체험 할 수 있는 여행 시뮬레이션 게임입니다.
공항, 대중교통, 호텔 등 여행 과정에서 경험 할 수 있는 일들을 체험하고 NPC와 직접 대화해 보면서 세계 여행에 대한 막연한 두려움을 줄이고 여행의 설렘과 자신감을 느낄 수 있습니다.

#### :movie_camera:[시연 영상](https://www.youtube.com/watch?v=yBlb9wq32ZM)


## 주요 기능

### <br>닉네임, 여행 국가, 스케쥴 설정

- 시작 전에 플레이어는 이름과 여행할 국가 그리고 스케쥴을 설정합니다.
- 설정한 국가와 스케쥴에 따라 시뮬레이션이 진행됩니다.

  <img src="https://github.com/user-attachments/assets/1de5b55f-c234-4efd-a73f-38de04703c58" width="40%" height="40%"/>
  <img src="https://github.com/user-attachments/assets/87b68073-2bea-4c28-9e97-26d4d3da4730" width="40%" height="40%"/>
  

### <br>NPC 대화 시스템

- 플레이어는 NPC에 가까이 다가가면 중앙의 마이크 버튼을 통해 NPC와 대화할 수 있습니다.
- "마이크로 대화 녹음 → AI 서버로 전송 및 응답 대기 → NPC 대답" 순서로 대화가 진행되게 됩니다.

  <img src="https://github.com/user-attachments/assets/238272b9-9253-442e-b7f8-66f45adfea21" width="70%" height="70%"/>


### <br>핸드 트래킹

- 플레이어는 핸드 트래킹을 사용하여 게임을 진행합니다.
- 예를 들어 오른손으로 엄지를 올린 자세를 취하면 이동 및 화전을 할 수 있고 왼손으로 손을 펼치면 핸드폰을 볼 수 있는 등 손을 사용해 게임 플레이에 필요한 기능들을 사용할 수 있습니다.

  <img src="https://github.com/user-attachments/assets/2d5fa090-c55d-4f6d-a92c-1cda0b6eadd5" width="30%" height="30%"/>
   <img src="https://github.com/user-attachments/assets/be91495c-8c54-4ac0-9bcf-027d91a27e25" width="30%" height="30%"/>
    <img src="https://github.com/user-attachments/assets/e8417fcc-b35e-4574-806b-b0696cbe9d73" width="30%" height="30%"/>


### <br>퀘스트 시스템
- 플레이어에게 각 장소에서 해야할 일을 안내해주는 퀘스트 시스템이 구현되어있습니다.
- 핸드폰을 통해 퀘스트를 확인할 수 있고 이를 수행하면 다음 퀘스트로 넘어갑니다.

  <img src="https://github.com/user-attachments/assets/5b029fea-4e74-4b7a-a7c0-e194e50c3ee6" width="70%" height="70%"/>
  

## <br>사용 기술
Client : Unity, C#, XR Interaction Toolkit

Server : FastAPI, Python

AI : OpenAI Whisper, Qwen, Coqui


## <br>팀원 소개
| 김범수 <a href="https://github.com/Starbow-Break"><img src="https://github.com/user-attachments/assets/5cf4751a-cd8d-4328-b893-d8f76379e049" width="16" height="16"/></a> | 박남훈 <a href="https://github.com/gunmango"><img src="https://github.com/user-attachments/assets/5cf4751a-cd8d-4328-b893-d8f76379e049" width="16" height="16"/></a> | 홍채영 <a href="https://github.com/kaiworld97"><img src="https://github.com/user-attachments/assets/5cf4751a-cd8d-4328-b893-d8f76379e049" width="16" height="16"/></a> | 김정현 <a href="https://github.com/kjhjeonghyeon"><img src="https://github.com/user-attachments/assets/5cf4751a-cd8d-4328-b893-d8f76379e049" width="16" height="16"/></a> | 한제성 |
|:---:|:---:|:---:|:---:|:---:|
| <img src="https://github.com/user-attachments/assets/2fdb6c87-57a7-4d4d-9552-b87ca5df228c" width="100" height="100"/> | <img src="https://github.com/user-attachments/assets/0ad6068f-0807-404c-87a0-876a52928fbf" width="100" height="100"/> | <img src="https://github.com/user-attachments/assets/629f4ce2-469f-4307-a50a-e9567dabdb74" width="100" height="100"/> | <img src="https://github.com/user-attachments/assets/52d607c2-dff2-4ba8-a1e8-b0fdefae6ac9" width="100" height="100"> |<img src="https://github.com/user-attachments/assets/aeeec2ab-1df7-419a-834c-8bb73dd5ee7e" width="100" height="100"/> |
| Unity | Unity | Backend/AI | TA | TA |


