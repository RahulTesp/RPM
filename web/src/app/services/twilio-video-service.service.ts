import {
  Injectable,
  ElementRef,
  Renderer2,
  RendererFactory2,
} from '@angular/core';
import {
  connect,
  MediaStreamTrackPublishOptions,
  RemoteParticipant,
  RemoteVideoTrackPublication,
  VideoTrack,
} from 'twilio-video';
import { BehaviorSubject, Observable } from 'rxjs';
import { Router } from '@angular/router';
import { any } from 'lodash/fp';
import { Element } from 'chart.js';

@Injectable({
  providedIn: 'root',
})
export class TwilioVideoServiceService {
  remoteVideo: any;
  disableremotevideo: any;
  localVideo: any;
  previewing: boolean;
  msgSubject = new BehaviorSubject('');
  roomObj: any;
  microphone = true;
  cameraOnSwitch = false;
  isVideoOn = true;
  videoConnected = false;
  dialing = false;
  roomParticipants: any;
  primaryVideoView: any;
  remoteVideoDisable=false

  private renderer: Renderer2;


  constructor(
    private router: Router,
    private rendererFactory: RendererFactory2

  ) {
    this.renderer = rendererFactory.createRenderer(null, null);
  }
  private endVideoCall: () => void;
  disconnectCallOnParticipantDisconnect(fn: () => void) {
    this.endVideoCall = fn;
    this.videoConnected = false;

    // from now on, call myFunc wherever you want inside this service
  }

  getToken(username: any): Observable<any> {
    // return this.http.post('/abc', { uid: 'ashish' });
    return username;
  }

  cameraOff() {
    this.roomObj.localParticipant.videoTracks.forEach(function (
      videoTrack: any
    ) {
      videoTrack.track.disable();
    });
    //this.cameraOnSwitch = true;
    this.isVideoOn = false;
  }

  cameraOn() {
    this.roomObj.localParticipant.videoTracks.forEach(function (
      videoTrack: any
    ) {
      videoTrack.track.enable();
    });
    //this.cameraOnSwitch = false;
    this.isVideoOn = true;
  }
  mute() {
    this.roomObj.localParticipant.audioTracks.forEach(function (
      audioTrack: any
    ) {
      audioTrack.track.disable();
    });
    this.microphone = false;
  }

  unmute() {
    this.roomObj.localParticipant.audioTracks.forEach(function (
      audioTrack: any
    ) {
      audioTrack.track.enable();
    });
    this.microphone = true;
  }
  // connectToRoom(accessToken: string, options:any): void {
  //   connect(accessToken, options).then(room => {}, (error) => {alert(error.message);});}

  connectToRoom(accessToken: string, options: any): void {
    connect(accessToken, options).then(
      (room) => {
        this.roomObj = room;
        this.videoConnected = true;
        this.dialing = false;
        if (!this.previewing && options['video']) {
          this.startLocalVideo();
          this.previewing = true;
        }

        this.roomParticipants = room.participants;

        room.participants.forEach((participant) => {
          this.attachParticipantTracks(participant);
        });

        room.on('participantConnected', (participant) => {
          this.roomParticipants = room.participants;
          this.attachParticipantTracks(participant);
          console.log('Room On Participant');
          console.log(participant);
          // participant.on('trackPublished', (track) => {
          //   console.log('Room Track Participant');
          //   const element = (track as any).attach();

          //   this.renderer.data.id = (track as any).sid;
          //   this.renderer.setStyle(element, 'height', '100%');

          //   this.renderer.setStyle(element, 'min-width', '100%');
          //   this.renderer.appendChild(this.remoteVideo.nativeElement, element);
          // });
          participant.on('trackPublished', track => {
            //this.remoteVideoDisable=false

            if(track.kind == "video"){
              console.log("Room Track Participant")
              //remove 'newly_content' --> (how to remove element from javascript)
              const newlyContentElement = document.getElementById('newly_content');

              // Check if the element exists
              if (newlyContentElement) {
                  // Remove the element from its parent node
                  const parentNode = newlyContentElement.parentNode;
                  if (parentNode) {
                      parentNode.removeChild(newlyContentElement);
                  }
              }
              this.attachParticipantTracks(participant);
            }
          });

          participant.on('trackDisabled', (track) => {
            if (track.kind == "video") {
              console.log("remote trackDisabled");
              let element = this.remoteVideo.nativeElement;
              // Find and remove only video elements
              const videoElements = element.querySelectorAll('video');
              videoElements.forEach((videoElement: any) => {
                element.removeChild(videoElement);
              });

              // Calculate 40% of viewport height for the placeholder
              const viewportHeight = window.innerHeight;
              const videoHeight = Math.round(viewportHeight * 0.4);
              const videoWidth = Math.round(videoHeight * (21/9));

              // Add the placeholder div with dynamic dimensions
              const newChild = document.createElement('div');
              newChild.id = 'newly_content';
              newChild.innerHTML = `
                <div style="background-color: black; height: ${videoHeight}px; width: ${videoWidth}px; max-width: 100%; border-radius: 8px; display: flex; align-items: center; justify-content: center;">
                  <span style="color: white; font-size: 16px;">Video Off</span>
                </div>
              `;
              this.renderer.appendChild(this.remoteVideo.nativeElement, newChild);
            }
          });

          // participant.on('trackDisabled', (track) => {
          //   if (track.kind == "video") {
          //     console.log("remote trackDisabled");
          //     let element = this.remoteVideo.nativeElement;
          //     // Find and remove only video elements
          //     const videoElements = element.querySelectorAll('video');
          //     videoElements.forEach((videoElement: any) => {
          //       element.removeChild(videoElement);
          //     });
          //     // Add the placeholder div
          //     const newChild = document.createElement('div');
          //     newChild.id = 'newly_content';
          //     newChild.innerHTML = `
          // <div style="background-color: black; height: 430px; width: 320px;">
          // </div>
          //     `;
          //     this.renderer.appendChild(this.remoteVideo.nativeElement, newChild);
          //   }
          // });
          participant.on('trackEnabled', track => {
            //this.remoteVideoDisable=false

            if(track.kind == "video"){
              console.log("remote trackEnabled")
              //remove 'newly_content' --> (how to remove element from javascript)
              const newlyContentElement = document.getElementById('newly_content');

              // Check if the element exists
              if (newlyContentElement) {
                  // Remove the element from its parent node
                  const parentNode = newlyContentElement.parentNode;
                  if (parentNode) {
                      parentNode.removeChild(newlyContentElement);
                  }
              }
              this.attachParticipantTracks(participant);
            }
          });

        });
        room.on('participantDisconnected', (participant) => {


          //change
          this.detachTracks(participant);
          this.endVideoCall();
          room.disconnect();
          //this.detachParticipantTracks(participant);
        });

        // When a Participant adds a Track, attach it to the DOM.
        room.on('trackPublished', (track, participant) => {
          console.log('Room Track trackPublished');
          //change
          //this.attachTracks([track]);

          if(track.kind == "video"){
            console.log("remote trackEnabled published ")
            //remove 'newly_content' --> (how to remove element from javascript)
            const newlyContentElement = document.getElementById('newly_content');

            // Check if the element exists
            if (newlyContentElement) {
                // Remove the element from its parent node
                const parentNode = newlyContentElement.parentNode;
                if (parentNode) {
                    parentNode.removeChild(newlyContentElement);
                }
            }
            this.attachParticipantTracks(participant);
          }



          // this.attachParticipantTracks(track);
        });
        //this.roomObj.remoteParticipant.on('trackDisabled', (track: any) => {
          //console.log("hoi hoi")
        //});
        // When a Participant removes a Track, detach it from the DOM.

        // room.on('trackRemoved', (track :any, participant:any) => {
          // this.detachTracks([track]);
        // });

        // amitha - rangeeth
        // Listen for the 'trackUnpublished' event
        room.on('trackUnpublished', (track, participant) => {
          console.log('Room Track trackUnpublished');

          // Handle the unpublished track here
        });






        room.once('disconnected', (room) => {
          room.localParticipant.tracks.forEach(
            (track: { track: { stop: () => void; detach: () => any } }) => {
              track.track.stop();
              const attachedElements = track.track.detach();

              attachedElements.forEach((element: { remove: () => any }) =>
                element.remove()
              );
              room.localParticipant.videoTracks.forEach((video: any) => {
                const trackConst = [video][0].track;
                trackConst.stop(); // <- error

                trackConst
                  .detach()
                  .forEach((element: { remove: () => any }) =>
                    element.remove()
                  );
                room.localParticipant.unpublishTrack(trackConst);
              });



              let element = this.remoteVideo.nativeElement;
              while (element.firstChild) {
                element.removeChild(element.firstChild);
              }
              let localElement = this.localVideo.nativeElement;
              while (localElement.firstChild) {
                localElement.removeChild(localElement.firstChild);
              }
              // this.router.navigate(['thanks']);
            }
          );
        });
        this.isVideoOn = true
      },
      (error) => {
        alert(error.message);
        this.dialing = false;
      }
    );
  }

  attachParticipantTracks(participant: any): void {
    participant.tracks.forEach((part: any) => {
      this.trackPublished(part);
    });
  }

  trackPublished(publication: any) {
    if (publication.isSubscribed) this.attachTracks(publication.track);

    if (!publication.isSubscribed)
      publication.on('subscribed', (track: any) => {
        this.attachTracks(track);
      });
  }

  attachTracks(tracks: any) {
    // console.log('tracks');
    // console.log(tracks);
    // const element = tracks.attach();
    // this.renderer.data.id = tracks.sid;
    // this.renderer.setStyle(element, 'height', '90%');
    // this.renderer.setStyle(element, 'min-width', '100%');
    //  this.renderer.setStyle(element, 'object-fit:', 'contain');

    // this.renderer.appendChild(this.remoteVideo.nativeElement, element);
    console.log('tracks');
  console.log(tracks);
  const element = tracks.attach();
  this.renderer.data.id = tracks.sid;

  // Calculate 40% of viewport height for a shorter video
  const viewportHeight = window.innerHeight;
  const videoHeight = Math.round(viewportHeight * 0.9);

  // Use a wider aspect ratio (21:9)
  const videoWidth = Math.round(videoHeight * (21/9));

  // Apply the new dimensions
  this.renderer.setStyle(element, 'height', `${videoHeight}px`);
  this.renderer.setStyle(element, 'min-width', '100%');
  this.renderer.setStyle(element, 'max-width', '100%');
  this.renderer.setStyle(element, 'object-fit', 'cover'); // Use cover to maintain aspect ratio
  this.renderer.setStyle(element, 'border-radius', '8px'); // Optional: rounded corners

  this.renderer.appendChild(this.remoteVideo.nativeElement, element);
  }

  startLocalVideo(): void {
    this.roomObj.localParticipant.videoTracks.forEach(
      (publication: { track: { attach: () => any; sid: any } }) => {
        const element = publication.track.attach();
        this.renderer.data.id = publication.track.sid;
        this.renderer.setStyle(element, 'width', '50%');
        this.renderer.appendChild(this.localVideo.nativeElement, element);
      }
    );
  }

  detachTracks(tracks: any): void {
    tracks.tracks.forEach((track: any) => {
      let element = this.remoteVideo.nativeElement;
      while (element.firstChild) {
        element.removeChild(element.firstChild);
      }
    });
  }
  detachParticipantTracks(participant: any) {
    this.detachTracks(participant);
  }
  // screenShare ()
  //  {
  //   navigator.mediaDevices
  //   .getDisplayMedia({
  //     audio: false,
  //     video: {
  //       frameRate: 10,
  //       height: 1080,
  //       width: 1920,
  //     },
  //   })
  //   .then(stream => {
  //     const track = stream.getTracks()[0];

  //     // All video tracks are published with 'low' priority. This works because the video
  //     // track that is displayed in the 'MainParticipant' component will have it's priority
  //     // set to 'high' via track.setPriority()
  //     this.roomObj.localParticipant
  //       .publishTrack(track, {
  //         name: 'screen', // Tracks can be named to easily find them later
  //         priority: 'low', // Priority is set to high by the subscriber when the video track is

  //       } as MediaStreamTrackPublishOptions)
  //       .then((trackPublication: any) => {
  //         this.localVideo.current = () => {
  //           this.roomObj.localParticipant.unpublishTrack(track);
  //           this.roomObj.localParticipant.emit('trackUnpublished', trackPublication);
  //           track.stop();
  //         };

  //         // track.onended = stopScreenShareRef.current;
  //         track.onended = this.localVideo.current;

  //       })
  //   })

  //  }

  trackRemoved(track:any) {
    track.detach().forEach( function(element:any) { element.remove() });
}

}
