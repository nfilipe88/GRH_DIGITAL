import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Alterarpassword } from './alterarpassword';

describe('Alterarpassword', () => {
  let component: Alterarpassword;
  let fixture: ComponentFixture<Alterarpassword>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Alterarpassword]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Alterarpassword);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
