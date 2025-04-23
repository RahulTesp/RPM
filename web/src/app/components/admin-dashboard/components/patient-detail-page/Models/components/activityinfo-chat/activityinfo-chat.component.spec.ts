import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ActivityinfoChatComponent } from './activityinfo-chat.component';

describe('ActivityinfoChatComponent', () => {
  let component: ActivityinfoChatComponent;
  let fixture: ComponentFixture<ActivityinfoChatComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ActivityinfoChatComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ActivityinfoChatComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
