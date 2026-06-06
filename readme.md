# Projeto Final - Sistemas de Redes para Jogos

## Autoria:

- Dinis Barroso - a22405350

## Introdução:

Neste projeto tentei recriar o minijogo de Kingdom Hearts 2, **Struggle Battles**, em formato 2D e Multiplayer de **ação**.

![Jogo Original](jogo_og.png)

A ideia do jogo é haverem dois jogadores, presos numa arena fechada, a tentaram roubar as bolas um do outro, atacando-se. 

Quando um jogador é atacado, fica num estado temporário de "stun" e larga uma bola que saltita pelo mapa, até ficar sem fricção.

Ao fim de 2 minutos, o jogador com o maior número de bolas vence.

![Recriação](recriacao.png)

Ao contrário do jogo original, nesta versão cada jogador tem 40 bolas e as partidas têm maior tempo de duração.

## Descrição técnica

Para desenvolvimento, decidi utilizar a biblioteca **Photon PUN 2**, e criar o projeto com tecnologia **Peer-to-Peer** com **Netcode baseado em delay**, [que é o tipo de redes que este tipo de jogos utilizam, sem contar o Rollback.](https://glossary.infil.net/?t=Delay-Based%20Netcode)

No menu, existem duas opções para partidas, um modo que permite jogar com um amigo via código, e um modo de Matchmaking que obtém a lista de partidas e tenta entrar numa, ou criar se não existirem partidas abertas.

O botão de Matchmaking não funciona com partidas feitas no botão "Host" por design.

![Menu](menu.png)

Ao começar ou entrar numa partida, o jogador é levado para a sala, onde começa o temporizador global caso haja outro jogador.

[Usei este vídeo no começo para o menu de matchmaking](https://youtu.be/C6dXcMo2x40?si=s3NYeI0noPWLfEH7) + [a documentação sobre Matchmaking do Photon.](https://doc.photonengine.com/realtime/current/lobby-and-matchmaking/matchmaking-and-lobby)

Também defino o "Rate" de envio e de serialização para 60, que é o normal em jogos de luta, pois antes dessa mudança, o jogo tinha pior preformance.

![Jogo](jogo1.png)

Durante a partida, o primeiro jogador age como **servidor**, sendo o mesmo que gere o temporizador, as pontuações e GameObjects.

O segundo jogador apenas pede ao servidor para incrementar a sua pontuação, instanciar e remover objetos, enviando informações sobre o seu estado, o seu **Rigidbody2D**, e da sua equipa, recebendo o resto das informações.

Cada jogador gere as suas próprias físicas e colisões, enviando-as um ao outro, com o servidor a gerir as físicas das bolas.

Quando um jogador recebe posições do outro ou das bolas, tenta calcular o tempo da mensagem com o do servidor, e assim adivinha a posição atual, sendo esta técnica chamada de [**Lag Compensation**](https://doc.photonengine.com/pun/current/gameplay/lagcompensation).

De seguida, em vez de teletransportar o **Rigidbody2D**, tenta movê-lo para a posição, com uma velocidade própria.

![Vencedor](vencedor.png)
Quando o temporizador do servidor acaba, o mesmo envia aos jogadores (A ele próprio e ao outro cliente) uma mensagem a sinalizar o fim de jogo e a indicar que equipa venceu.