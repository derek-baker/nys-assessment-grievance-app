import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FakePostComponent } from './fake-post.component';

describe('FakePostComponent', () => {
  let component: FakePostComponent;
  let fixture: ComponentFixture<FakePostComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FakePostComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FakePostComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
